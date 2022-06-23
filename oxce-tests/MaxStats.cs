using System;
using System.Collections.Generic;
using System.Linq;

namespace OxceTests
{
    public static class MaxStats
    {
        public static IEnumerable<string> CsvHeaders() => new[]
        {
            "MaxTU",
            "MaxStamina",
            "MaxHealth",
            "MaxBravery",
            "MaxReactions",
            "MaxFiring",
            "MaxThrowing",
            "MaxStrength",
            "MaxMelee",
            "MaxMana",
            "MaxTotal",
            "FullyMaxxed"
        };

        public static IEnumerable<(string Key, object Value)> Get(Soldier soldier)
        {
            var caps = StatCaps.MapByType[soldier.Type];
            var values = new List<int>
            {
                Math.Min(soldier.CurrentStats.TU - caps.TU, 0),
                Math.Min(soldier.CurrentStats.Stamina - caps.Stamina, 0),
                Math.Min(soldier.CurrentStats.Health - caps.Health, 0),
                Math.Min(soldier.CurrentStats.Bravery - caps.Bravery, 0) / 10,
                Math.Min(soldier.CurrentStats.Reactions - caps.Reactions, 0),
                Math.Min(soldier.CurrentStats.Firing - caps.Firing, 0),
                Math.Min(soldier.CurrentStats.Throwing - caps.Throwing, 0),
                Math.Min(soldier.CurrentStats.Strength - caps.Strength, 0),
                Math.Min(soldier.CurrentStats.Melee - caps.Melee, 0),
                Math.Min(soldier.CurrentStats.Mana - caps.Mana, 0)
            };

            var maxTotal = values.Sum();
            var derivedData = new (string, object)[]
            {
                ("MaxTotal", maxTotal),
                ("FullyMaxxed", maxTotal == 0 ? "TRUE" : "FALSE")
            };

            var allData = CsvHeaders().Zip(values)
                .Select(pair => (pair.First, (object)pair.Second))
                .Union(derivedData); // kja should be Concat instead?

            return allData;
        }
    }
}