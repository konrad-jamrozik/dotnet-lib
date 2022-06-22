using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record SoldierBonuses(IEnumerable<SoldierBonus> SoldierBonusData) : IEnumerable<SoldierBonus>
{
    public static SoldierBonuses FromRulesetFileYaml(YamlMapping soldierBonusesYaml)
    {
        var soldierBonusesLines =
            new YamlBlockSequence(soldierBonusesYaml.Lines("soldierBonuses")).NodesLines();
        var soldierBonuses = soldierBonusesLines.Select(
            lines => SoldierBonus.FromSoldierYaml(new YamlMapping(lines)));
        return new SoldierBonuses(soldierBonuses);
    }

    public IEnumerator<SoldierBonus> GetEnumerator() => SoldierBonusData.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}