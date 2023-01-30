using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class MissionSite : ITemporal, IResettable
{
    public MissionSiteData Data => _missionSitesData.Data[0];

    public FactionData FactionData => _factionsData.Data.Single(f => f.Name == Data.FactionName);

    public int Countdown => CurrentlyAvailable ? -Data.ExpiresIn : Data.AvailableIn;

    public bool CurrentlyAvailable => Data.AvailableIn == 0 && Data.ExpiresIn > 0;
    
    public bool MissionAboutToExpire => CurrentlyAvailable && Data.ExpiresIn == 1;

    private readonly RandomGen _randomGen;
    private readonly ArchiveData _archiveData;
    private readonly FactionsData _factionsData;
    private readonly PlayerScore _playerScore;
    private readonly MissionSitesData _missionSitesData;
    public readonly MissionStats MissionStats;

    public MissionSite(
        MissionSitesData missionSitesData,
        ArchiveData archiveData,
        FactionsData factionsData,
        PlayerScore playerScore,
        MissionStats missionStats,
        RandomGen randomGen)
    {
        _missionSitesData = missionSitesData;
        _archiveData = archiveData;
        _factionsData = factionsData;
        _playerScore = playerScore;
        MissionStats = missionStats;
        _randomGen = randomGen;
        if (Data.IsNoMission)
            _missionSitesData.New(_playerScore, _randomGen, _factionsData);
    }

    public void AdvanceTime()
    {
        Debug.Assert(!_playerScore.GameOver);
        if (CurrentlyAvailable)
        {
            Debug.Assert(Data.ExpiresIn >= 1);
            if (MissionAboutToExpire)
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

    public void Reset()
    {
        _missionSitesData.Reset();
        GenerateNewOrClearMission();
    }

    public void GenerateNewOrClearMission()
        => _missionSitesData.New(_playerScore, _randomGen, _factionsData);
}