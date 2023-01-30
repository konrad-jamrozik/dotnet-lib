using UfoGame.Model;
using UfoGame.Model.Data;

namespace UfoGame.Infra;

public static class ServiceCollectionExtensions
{
    private static readonly Type[] InterfaceTypes = new Type[]
        { typeof(IPersistable), typeof(IResettable), typeof(ITemporal) };

    /// <summary>
    /// Calls:
    ///
    ///   services.AddSingleton(type, instance)
    ///
    /// but also makes a set of calls of form:
    ///
    ///   services.AddSingleton(typeof(IFoo), instance)
    ///
    /// for each value of IFoo, if the type is assignable to IFoo.
    ///
    /// There are few values of IFoo supported - see the implementation.
    ///
    /// The purpose of these interface injection is to add to the DI registrations
    /// a collection of all types implementing given interface. This way such
    /// collection can be injected into ctor of given class during resolution, allowing
    /// the class to enumerate all registered instances of the interface and execute a method
    /// on them, without having to know the specific types implementing given interface, thus
    /// needing to have them injected.
    ///
    /// For example, if IFoo is IResettable, then a class may have IEnumerable of IResettable
    /// injected and then call Reset() on all such injected instances. Without this, if there
    /// would be 5 different types implementing this interface, all 5 of them would have
    /// to be injected into the ctor.
    ///
    /// As a consequence of supporting registering and resolving of such collection of IResettable,
    /// adding a new IResettable class to the code boils down to ensuring it
    /// implements the IResettable interface, and that's it. No need to add that
    /// new class as a ctor param to the method that is supposed to call Reset()
    /// on all classes implementing IResettable.
    /// </summary>
    /// <param name="services">Services to which the "instance" is to be registered as "type".</param>
    /// <param name="type">Type of the registered "instance".</param>
    /// <param name="instance">The registered instance, of type "type".</param>
    public static void AddSingletonWithInterfaces(this IServiceCollection services, Type type, object instance)
    {
        services.AddSingleton(type, instance);
        // Implementation based on https://stackoverflow.com/a/39569277/986533
        if (type.IsAssignableTo(typeof(IPersistable)))
            services.AddSingleton(typeof(IPersistable), instance);
        if (type.IsAssignableTo(typeof(IResettable)))
            services.AddSingleton(typeof(IResettable), instance);
        if (type.IsAssignableTo(typeof(ITemporal)))
            services.AddSingleton(typeof(ITemporal), instance);
    }

    /// <summary>
    /// This method resolves instance of type 'type' from 'serviceProvider'
    /// and registers it as a singleton in 'services', in addition to registering
    /// it as all applicable IFoo interfaces it implements, the same way it is
    /// done in the other overload of this method.
    ///
    /// Note this maneuver of first resolving an instance and then registering
    /// it is necessary; otherwise, if the the registrations for the IFoo interfaces
    /// would be made directly using just the type, without the instance,
    /// it would lead to creation of multiple instances (one per each IFoo interface
    /// plus the non-interface one) instead of just one.
    /// </summary>
    public static void AddSingletonWithInterfaces(
        this IServiceProvider serviceProvider,
        IServiceCollection services,
        Type type)
    {
        if (!InterfaceTypes.Any(type.IsAssignableTo))
            return;

        object service = serviceProvider.GetService(type)!;
        services.AddSingleton(type, service);
        // Implementation based on https://stackoverflow.com/a/39569277/986533
        foreach (var interfaceType in InterfaceTypes)
            if (type.IsAssignableTo(interfaceType))
                services.AddSingleton(interfaceType, service);
    }
}

