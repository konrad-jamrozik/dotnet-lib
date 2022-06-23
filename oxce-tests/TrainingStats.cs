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
            "TrainTotal",
            "FullyTrained",
            "TrainPsiSkill",
            "PsiTrained"
        };

        public static IEnumerable<(string Key, object Value)> Get(Soldier soldier)
        {
            var caps = TrainingStatCaps.MapByType[soldier.Type];
            var values = new List<int>
            {
                Math.Min(soldier.CurrentStats.TU - caps.TU, 0),
                Math.Min(soldier.CurrentStats.Stamina - caps.Stamina, 0),
                Math.Min(soldier.CurrentStats.Health - caps.Health, 0),
                Math.Min(soldier.CurrentStats.Firing - caps.Firing, 0),
                Math.Min(soldier.CurrentStats.Throwing - caps.Throwing, 0),
                Math.Min(soldier.CurrentStats.Strength - caps.Strength, 0),
                Math.Min(soldier.CurrentStats.Melee - caps.Melee, 0),
            };

            var trainTotal = values.Sum();
            var derivedData = new (string, object)[]
            {
                ("TrainTotal", trainTotal),
                ("FullyTrained", trainTotal == 0 ? "TRUE" : "FALSE"),
                ("TrainPsiSkill", Math.Min(soldier.CurrentStats.PsiSkill - caps.PsiSkill, 0)),
                ("PsiTrained", soldier.CurrentStats.PsiSkill - caps.PsiSkill < 0 ? "FALSE" : "TRUE")
            };

            var allData = CsvHeaders().Zip(values)
                .Select(pair => (pair.First, (object)pair.Second))
                .Union(derivedData); // kja should be Concat instead?

            return allData;
        }
    }
}