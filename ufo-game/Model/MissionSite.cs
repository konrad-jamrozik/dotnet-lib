using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class MissionSite : ITemporal, IResettable
{
    public readonly MissionStats MissionStats;

    private readonly MissionSitesData _missionSitesData;
    private readonly FactionsData _factionsData;
    private readonly ArchiveData _archiveData;
    private readonly PlayerScore _playerScore;
    private readonly RandomGen _randomGen;

    public MissionSite(
        MissionStats missionStats,
        MissionSitesData missionSitesData,
        FactionsData factionsData,
        ArchiveData archiveData,
        PlayerScore playerScore,
        RandomGen randomGen)
    {
        MissionStats = missionStats;
        _missionSitesData = missionSitesData;
        _factionsData = factionsData;
        _archiveData = archiveData;
        _playerScore = playerScore;
        _randomGen = randomGen;
        if (Data.IsNoMission)
            _missionSitesData.New(_playerScore, _randomGen, _factionsData);
    }

    public MissionSiteData Data => _missionSitesData.Data[0];

    public FactionData FactionData => _factionsData.Data.Single(f => f.Name == Data.FactionName);

    public int Countdown => CurrentlyAvailable ? -Data.ExpiresIn : Data.AvailableIn;

    public bool CurrentlyAvailable => Data.AvailableIn == 0 && Data.ExpiresIn > 0;

    public bool AboutToExpire => CurrentlyAvailable && Data.ExpiresIn == 1;

    public void GenerateNewOrClearMission()
        => _missionSitesData.New(_playerScore, _randomGen, _factionsData);

    public void Reset()
    {
        _missionSitesData.Reset();
        GenerateNewOrClearMission();
    }

    public void AdvanceTime()
    {
        // Console.WriteLine(
        //     $"MissionSite.AdvanceTime Data.AvailableIn {Data.AvailableIn} Data.ExpiresIn {Data.ExpiresIn} " +
        //     $"CurrentlyAvailable {CurrentlyAvailable} AboutToExpire {AboutToExpire}");
        Debug.Assert(!_playerScore.GameOver);
        if (CurrentlyAvailable)
        {
            Debug.Assert(Data.ExpiresIn >= 1);
            if (AboutToExpire)
            {
                _archiveData.RecordIgnoredMission();
                _playerScore.Data.Value -= PlayerScore.IgnoreMissionScoreLoss;
                GenerateNewOrClearMission();
            }
            else
                Data.ExpiresIn--;
        }
        else
        {
            Debug.Assert(Data.AvailableIn >= 1);
            Data.AvailableIn--;
            if (CurrentlyAvailable)
            {
                if (!FactionData.Discovered)
                {
                    Console.Out.WriteLine("Discovered faction! " + FactionData.Name);
                    FactionData.Discovered = true;
                }
            }
        }
    }
}