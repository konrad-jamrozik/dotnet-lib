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
                Math.Min(soldier.CurrentTU - caps.TU, 0),
                Math.Min(soldier.CurrentStamina - caps.Stamina, 0),
                Math.Min(soldier.CurrentHealth - caps.Health, 0),
                Math.Min(soldier.CurrentFiring - caps.Firing, 0),
                Math.Min(soldier.CurrentThrowing - caps.Throwing, 0),
                Math.Min(soldier.CurrentStrength - caps.Strength, 0),
                Math.Min(soldier.CurrentMelee - caps.Melee, 0),
                Math.Min(soldier.CurrentMana - caps.Mana, 0)
            };

            var trainTotal = values.Sum();
            var derivedData = new (string, object)[]
            {
                ("MaxTotal", trainTotal),
                ("FullyMaxxed", trainTotal == 0 ? "TRUE" : "FALSE")
            };

            var allData = CsvHeaders().Zip(values)
                .Select(pair => (pair.First, (object)pair.Second))
                .Union(derivedData);

            return allData;
        }
    }
}