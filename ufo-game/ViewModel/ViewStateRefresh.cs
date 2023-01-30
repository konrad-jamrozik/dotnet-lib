using System.Runtime.CompilerServices;

namespace UfoGame.ViewModel;

/// <summary>
/// Implements "In-memory container service" pattern:
/// https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-7.0&pivots=server#in-memory-state-container-service-server
///
/// See also:
/// https://learn.microsoft.com/en-us/aspnet/core/blazor/components/rendering?view=aspnetcore-7.0#to-render-a-component-outside-the-subtree-thats-rerendered-by-a-particular-event
/// </summary>
public class ViewStateRefresh
{
    public Action Event = () => { };

    public void Trigger([CallerMemberName] string callerMemberName = "") 
    {
        Console.Out.WriteLine("REFRESH - triggering from " + callerMemberName);
        Event();
    }
}
