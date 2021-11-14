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
                Math.Min(soldier.CurrentTU - caps.TU, 0),
                Math.Min(soldier.CurrentStamina - caps.Stamina, 0),
                Math.Min(soldier.CurrentHealth - caps.Health, 0),
                Math.Min(soldier.CurrentFiring - caps.Firing, 0),
                Math.Min(soldier.CurrentThrowing - caps.Throwing, 0),
                Math.Min(soldier.CurrentStrength - caps.Strength, 0),
                Math.Min(soldier.CurrentMelee - caps.Melee, 0),
            };

            var trainTotal = values.Sum();
            var derivedData = new (string, object)[]
            {
                ("TrainTotal", trainTotal),
                ("FullyTrained", trainTotal == 0 ? "TRUE" : "FALSE"),
                ("TrainPsiSkill", Math.Min(soldier.CurrentPsiSkill - caps.PsiSkill, 0)),
                ("PsiTrained", soldier.CurrentPsiSkill - caps.PsiSkill < 0 ? "FALSE" : "TRUE")
            };

            var allData = CsvHeaders().Zip(values)
                .Select(pair => (pair.First, (object)pair.Second))
                .Union(derivedData);

            return allData;
        }
    }
}