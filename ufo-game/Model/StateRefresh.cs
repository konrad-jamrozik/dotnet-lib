namespace UfoGame.Model;

public class StateRefresh
{
    public Action Refresh = () =>
    {
        Console.Out.WriteLine("On refresh!");
    };
}
