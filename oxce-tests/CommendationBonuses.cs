using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record CommendationBonuses(
    Dictionary<string, string[]> CommendationNameToSoldierBonusTypesMap,
    Dictionary<string, SoldierStats> SoldierBonusNameToStatsMap)
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
        var soldierBonuses = new List<SoldierStats>();

        foreach (var (name, decoration) in soldier.Diary.Commendations)
        {
            var nameWithoutNoun = name.Split("/").First();
            var soldierBonusTypes = CommendationNameToSoldierBonusTypesMap[nameWithoutNoun];
            var soldierBonusType = soldierBonusTypes[decoration];
            var statsData = SoldierBonusNameToStatsMap[soldierBonusType];
            soldierBonuses.Add(statsData);
        }

        foreach (var name in soldier.TransformationBonuses.TransformationNames)
        {
            if (SoldierBonusNameToStatsMap.ContainsKey(name))
            {
                soldierBonuses.Add(SoldierBonusNameToStatsMap[name]);
            }
            else
            {
                // Do nothing.
                //
                // This case happens when there is no entry in soldierBonuses_XCOMFILES.rul.
                // This is the only file we care about, because only this file has dynamically
                // added bonus stats, which are not reflected in the save file.
                //
                // Note that there still might entry with this name in soldierTransformation_XCOMFILES.rul,
                // but we do not care about it, because any stats changes coming from this file are
                // reflected in the data in the save file.
                //
                // One known example of this is STR_COMBAT_PILOT_TRAINING.
            }
        }

        var soldierWithBonuses = soldier; // with { CurrentMana = soldier.CurrentMana + soldierBonuses.Sum(sb => sb.Mana) };
        return soldierWithBonuses;
    }
}