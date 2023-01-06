﻿using System.Diagnostics;
using System.Text.Json.Nodes;

namespace UfoGame.Model;

public class Game
{
    public int MoneyRaisedAmount = 50;
    // Currently zero, as it offsets costs of actions, resulting in confusing
    // balance.
    public const int MoneyPerTurnAmount = 0;
    
    public int MoneyRaisingMethodsResearchCost = 100;
    public const int MoneyRaisingMethodsResearchCostIncrement = 10;

    public int SoldierEffectivenessResearchCost = 100;
    public const int SoldierEffectivenessResearchCostIncrement = 10;

    public int SoldierSurvivabilityResearchCost = 100;
    public const int SoldierSurvivabilityResearchCostIncrement = 10;

    public readonly Timeline Timeline;
    public readonly Money Money;
    public readonly Staff Staff;
    public readonly OperationsArchive Archive;
    public readonly MissionPrep MissionPrep;
    public readonly PendingMission PendingMission;
    public readonly StateRefresh StateRefresh;
    public readonly Factions Factions;
    public readonly PlayerScore PlayerScore;
    public readonly PersistentStorage Storage;

    public Game(
        Timeline timeline,
        Money money,
        Staff staff,
        OperationsArchive archive,
        MissionPrep missionPrep,
        PendingMission pendingMission,
        StateRefresh stateRefresh,
        Factions factions,
        PlayerScore playerScore,
        PersistentStorage storage)
    {
        Timeline = timeline;
        Money = money;
        Staff = staff;
        Archive = archive;
        MissionPrep = missionPrep;
        PendingMission = pendingMission;
        StateRefresh = stateRefresh;
        Factions = factions;
        PlayerScore = playerScore;
        Storage = storage;
        LoadGameState();
    }

    public bool CanDoNothing() => !PlayerScore.GameOver;

    public void DoNothing() => AdvanceTime();

    public void AdvanceTime(bool addMoney = true)
    {
        Debug.Assert(!PlayerScore.GameOver);
        Timeline.IncrementTime();
        if (addMoney)
            Money.AddMoney(MoneyPerTurnAmount);
        PendingMission.AdvanceMissionTime();
        Factions.AdvanceFactionsTime();
        StateRefresh.Trigger();

        // kja experimental
        PersistGameState();

    }

    private void LoadGameState()
    {
        Console.Out.WriteLine("Loading game state");

        if (Storage.ContainKey(nameof(Timeline)))
        {
            Console.Out.WriteLine("Loading Timeline");
            var timeline = Storage.GetItem<Timeline>(nameof(Timeline));
            Timeline.CurrentTime = timeline.CurrentTime;
        }

        if (Storage.ContainKey(nameof(PendingMission)))
        {
            Console.Out.WriteLine("Loading PendingMission");
            PendingMission.Hydrate(Storage.GetItem<JsonNode>(nameof(PendingMission)));
        }
    }

    private void PersistGameState()
    {
        Console.Out.WriteLine("Persisting game state");
        var itemsToSave = new List<(string key, object value)>
        {
            ("Timeline", Timeline),
            ("PendingMission", PendingMission)
        };
        // foreach (var faction in Factions.Data)
        // {
        //     itemsToSave.Add(($"Faction.Name:\"{faction.Name}\"", faction));
        // }
        foreach (var item in itemsToSave)
        {
            Console.Out.WriteLine("Persisting item: " + item.key + " : " + item.value);
            Storage.SetItem(item.key, item.value);
        }
    }

    public bool CanRaiseMoney() => !PlayerScore.GameOver;

    public void RaiseMoney()
    {
        Money.AddMoney(MoneyRaisedAmount);
        AdvanceTime(addMoney: false);
    }

    public bool CanResearchMoneyRaisingMethods()
        => !PlayerScore.GameOver && Money.CurrentMoney >= MoneyRaisingMethodsResearchCost;

    public void ResearchMoneyRaisingMethods()
    {
        Debug.Assert(CanResearchMoneyRaisingMethods());
        Money.SubtractMoney(MoneyRaisingMethodsResearchCost);
        MoneyRaisingMethodsResearchCost += MoneyRaisingMethodsResearchCostIncrement;
        MoneyRaisedAmount += 5;
        AdvanceTime();
    }

    public bool CanResearchSoldierEffectiveness()
        => !PlayerScore.GameOver && Money.CurrentMoney >= SoldierEffectivenessResearchCost;

    public void ResearchSoldierEffectiveness()
    {
        Debug.Assert(CanResearchSoldierEffectiveness());
        Money.SubtractMoney(SoldierEffectivenessResearchCost);
        SoldierEffectivenessResearchCost += SoldierEffectivenessResearchCostIncrement;
        Staff.SoldierEffectiveness += 10;
        AdvanceTime();
    }

    public bool CanResearchSoldierSurvivability()
        => !PlayerScore.GameOver && Money.CurrentMoney >= SoldierSurvivabilityResearchCost;

    public void ResearchSoldierSurvivability()
    {
        Debug.Assert(CanResearchSoldierSurvivability());
        Money.SubtractMoney(SoldierSurvivabilityResearchCost);
        SoldierSurvivabilityResearchCost += SoldierSurvivabilityResearchCostIncrement;
        Staff.SoldierSurvivability += 10;
        AdvanceTime();
    }

    public void Reset()
    {
        Storage.Reset();
        Timeline.Reset();
        StateRefresh.Trigger();
    }
}
