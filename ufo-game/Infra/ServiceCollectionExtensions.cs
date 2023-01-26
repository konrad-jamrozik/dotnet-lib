using UfoGame.Model;
using UfoGame.Model.Data;

namespace UfoGame.Infra;

public static class ServiceCollectionExtensions
{
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
        if (type.IsAssignableTo(typeof(IDeserializable)))
            services.AddSingleton(typeof(IDeserializable), instance);
        if (type.IsAssignableTo(typeof(IResettable)))
            services.AddSingleton(typeof(IResettable), instance);
        if (type.IsAssignableTo(typeof(ITemporal)))
            services.AddSingleton(typeof(ITemporal), instance);
    }

    /// <summary>
    /// This method is analogous to its overload but with the "instance" parameter.
    /// This overload is registering the "type" type directly, instead of
    /// instance "instance" of the type "type".
    /// </summary>
    public static void AddSingletonWithInterfaces(this IServiceCollection services, Type type)
    {
        services.AddSingleton(type);
        // Implementation based on https://stackoverflow.com/a/39569277/986533
        if (type.IsAssignableTo(typeof(IDeserializable)))
            services.AddSingleton(typeof(IDeserializable), type);
        if (type.IsAssignableTo(typeof(IResettable)))
            services.AddSingleton(typeof(IResettable), type);
        if (type.IsAssignableTo(typeof(ITemporal)))
            services.AddSingleton(typeof(ITemporal), type);
    }
}

