using System;
using System.Collections;
using System.Collections.Generic;

namespace OxceTests;

public record SoldierBonuses(IEnumerable<SoldierBonus> SoldierBonusData) : IEnumerable<SoldierBonuses>
{

    public static SoldierBonuses FromRulesetFile(YamlMapping soldierBonusesYaml)
    {
        return null;
    }

    public IEnumerator<SoldierBonuses> GetEnumerator()
    {
        return null;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}