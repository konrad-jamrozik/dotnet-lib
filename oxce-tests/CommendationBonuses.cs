using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record CommendationBonuses(Commendations Commendations, SoldierBonuses SoldierBonuses)
{
    public static CommendationBonuses Build(
        Commendations commendations,
        SoldierBonuses soldierBonuses)
    {
        return new CommendationBonuses(commendations, soldierBonuses);
    }

    public Soldier AddToSoldier(Soldier soldier)
    {
        // kja fixups to AddToSoldier:
        // - improve perf: avoid doing .Single, instead, have a Dict
        // - get rid of foreachs: dedup, and convert to functional
        // - SoldierBonus.StatsData should be its own type, named Stats.

        var soldierBonuses = new List<SoldierBonus.StatsData>();

        // Uncomment when perf fixed
        // foreach (var (name, decoration) in soldier.Diary.Commendations)
        // {
        //     var nameWithoutNoun = name.Split("/").First();
        //
        //     var (_, soldierBonusTypes) = Commendations.CommendationData.Single(commendation => commendation.Type == nameWithoutNoun);
        //     string soldierBonusType = soldierBonusTypes[decoration];
        //     var (_, statsData) = SoldierBonuses.SoldierBonusData.Single(bonus => bonus.Name == soldierBonusType);
        //     soldierBonuses.Add(statsData);
        // }

        // kja commented out as I need to support soldierTransformations file, stuff like "flatOverallStatChange". See for example STR_COMBAT_PILOT_TRAINING
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