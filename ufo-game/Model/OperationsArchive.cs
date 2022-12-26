namespace UfoGame.Model;

public class OperationsArchive
{
    public int MissionsLaunched { get; private set; } = 0;

    public int SuccessfulMissions { get; private set; } = 0;

    public int FailedMissions { get; private set; } = 0;

    public int SoldiersLost { get; private set; } = 0;

    public void ArchiveMission(bool missionSuccessful)
    {
        MissionsLaunched += 1;
        if (missionSuccessful)
            SuccessfulMissions += 1;
        else
            FailedMissions += 1;
    }

    public void RecordLostSoldier()
    {
        SoldiersLost += 1;
    }
}
