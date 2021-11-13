using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OxceTests
{
    public record Soldier(
        string Name,
        string Type,
        string BaseName,
        int Missions,
        int Kills,
        int Rank,
        int MonthsService,
        int StatGainTotal,
        int CurrentTU,
        int CurrentStamina,
        int CurrentHealth,
        int CurrentBravery,
        int CurrentReactions,
        int CurrentFiring,
        int CurrentThrowing,
        int CurrentStrength,
        int CurrentPsiStrength,
        int CurrentPsiSkill,
        int CurrentMelee,
        int CurrentMana)
    {
        public float KillsPerMission => Missions >= 5 ? (float) Kills / Missions : 0;

        public int StatGain => Math.Max(StatGainTotal - CurrentPsiSkill, 0);

        public TrainingStatCaps TrainingStatCaps => TrainingStatCaps.MapByType[Type];

        public static string CsvHeaders()
            => string.Join(
                ",",
                PrintableProperties.Select(p => p.Name).Union(TrainingStats.CsvHeaders()));

        private static IEnumerable<PropertyInfo> PrintableProperties { get; } =
            typeof(Soldier).GetProperties().Where(p => p.Name != nameof(TrainingStatCaps));

        private Dictionary<string, object> PrintableKeyValuePairs()
        {
            var propertyData = PrintableProperties.Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(this)));

            IEnumerable<KeyValuePair<string, object>> trainingData = TrainingStats.Get(this);
            var allData = propertyData.Union(trainingData);
            return new Dictionary<string, object>(allData);
        }


        public override string ToString() => "Soldier { " + string.Join(
            ", ",
            PrintableKeyValuePairs().Select(p => $"{p.Key} = {p.Value}")) + " }";

        public string CsvString()
        {
            var str = ToString();
            var csvStr = Regex.Replace(str, "Soldier {(.*) }", "$1");
            csvStr = Regex.Replace(csvStr, " \\w+\\ = ", "");
            return csvStr;
        }
    }
}