using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record CommendationBonuses(
    Dictionary<string, string[]> CommendationNameToSoldierBonusTypesMap,
    Dictionary<string, SoldierBonus.StatsData> SoldierBonusNameToStatsMap)
{
    public static CommendationBonuses Build(
        Commendations commendations,
        SoldierBonuses soldierBonuses)
        => new CommendationBonuses(
            commendations.ToDictionary(
                commendation => commendation.Type,
                commendation => commendation.SoldierBonusTypes),
            soldierBonuses.ToDictionary(
                soldierBonus => soldierBonus.Name,
                soldierBonus => soldierBonus.Stats));

    public Soldier AddToSoldier(Soldier soldier)
    {
        // kja fixups to AddToSoldier:
        // - get rid of foreachs: dedup, and convert to functional
        // - SoldierBonus.StatsData should be its own type, named Stats.

        var soldierBonuses = new List<SoldierBonus.StatsData>();

        foreach (var (name, decoration) in soldier.Diary.Commendations)
        {
            var nameWithoutNoun = name.Split("/").First();
        
            var soldierBonusTypes = CommendationNameToSoldierBonusTypesMap[nameWithoutNoun];
            var soldierBonusType = soldierBonusTypes[decoration];
            var statsData = SoldierBonusNameToStatsMap[soldierBonusType];
            soldierBonuses.Add(statsData);
        }

        // kja commented out as I need to support soldierTransformations file / soldierBonusType. See for example STR_COMBAT_PILOT_TRAINING
        // foreach (string name in soldier.TransformationBonuses.TransformationNames)
        // {
        //     if (!SoldierBonuses.SoldierBonusData.Any(bonus => bonus.Name == name))
        //     {
        //         Debugger.Launch();
        //     }
        //     var (_, statsData) = SoldierBonuses.SoldierBonusData.Single(bonus => bonus.Name == name);
        //     soldierBonuses.Add(statsData);
        // }

        var soldierWithBonuses = soldier with { CurrentMana = soldier.CurrentMana + soldierBonuses.Sum(sb => sb.Mana) };
        return soldierWithBonuses;
    }
}