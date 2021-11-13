using System;
using System.Collections.Generic;
using System.Linq;

namespace OxceTests
{
    public static class TrainingStats
    {
        public static IEnumerable<string> CsvHeaders() => new[]
        {
            "TrainTU",
            "TrainStamina",
            "TrainHealth",
            "TrainFiring",
            "TrainThrowing",
            "TrainStrength",
            "TrainMelee",
            "TrainMana",
            "TrainTotal",
            "FullyTrained"
        };

        public static IEnumerable<KeyValuePair<string, object>> Get(Soldier soldier)
        {
            var values = new List<int>
            {
                Math.Min(soldier.CurrentTU - soldier.TrainingStatCaps.TU, 0),
                Math.Min(soldier.CurrentStamina - soldier.TrainingStatCaps.Stamina, 0),
                Math.Min(soldier.CurrentHealth - soldier.TrainingStatCaps.Health, 0),
                Math.Min(soldier.CurrentFiring - soldier.TrainingStatCaps.Firing, 0),
                Math.Min(soldier.CurrentThrowing - soldier.TrainingStatCaps.Throwing, 0),
                Math.Min(soldier.CurrentStrength - soldier.TrainingStatCaps.Strength, 0),
                Math.Min(soldier.CurrentMelee - soldier.TrainingStatCaps.Melee, 0),
                Math.Min(soldier.CurrentMana - soldier.TrainingStatCaps.Mana, 0)
            };

            var trainTotal = values.Sum();
            var derivedPairs = new (string, object)[]
            {
                ("TrainTotal", trainTotal),
                ("FullyTrained", trainTotal == 0 ? "TRUE" : "FALSE")
            };

            var allPairs = CsvHeaders().Zip(values)
                .Select(pair => (pair.First, (object)pair.Second))
                .Union(derivedPairs);
            
            return allPairs.Select(p => new KeyValuePair<string, object>(p.Item1, p.Item2));
        }
    }
}