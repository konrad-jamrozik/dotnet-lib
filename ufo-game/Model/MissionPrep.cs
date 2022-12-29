namespace UfoGame.Model;

public class MissionPrep
{
    private readonly StateRefresh _refresh;
    private int _soldiersToSend;

    public MissionPrep(StateRefresh refresh)
    {
        _refresh = refresh;
    }

    public int SoldiersToSend
    {
        get => _soldiersToSend;
        set
        {
            _soldiersToSend = value;
            _refresh.Refresh();
        }
    }
}
