namespace UfoGameLib;

public record Archive
{
    // kja this can be instead derived from GS.Assets.Agents.Count
    public int AgentsHiredCount { get; set; }

    // kja this soon will be derivable from GS.Missions.Count
    public int MissionsLaunchedCount { get; set; }
}