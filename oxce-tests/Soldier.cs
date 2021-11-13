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
        private static IEnumerable<PropertyInfo> Properties { get; } = typeof(Soldier).GetProperties();

        public static string CsvHeaders()
            => string.Join(",", 
                Properties.Select(p => p.Name)
                    .Union(TrainingStats.CsvHeaders())
                    .Union(MaxStats.CsvHeaders()));

        public float KillsPerMission => Missions >= 5 ? (float) Kills / Missions : 0;

        public int StatGainReal => Math.Max(StatGainTotal - CurrentPsiSkill, 0);

        public string Helix => Name.EndsWith("H") ? "TRUE" : "FALSE";

        public string Humanoid = Type is "STR_SOLDIER" or "STR_HYBRID" ? "TRUE" : "FALSE";

        public override string ToString() => $"{nameof(Soldier)} {{ " 
            + string.Join(", ", DataMap().Select(p => $"{p.Key} = {p.Value}")) + " }";

        public string CsvString()
        {
            var str = ToString();
            var csvStr = Regex.Replace(str, $"{nameof(Soldier)} {{(.*) }}", "$1");
            csvStr = Regex.Replace(csvStr, " \\w+\\ = ", "");
            return csvStr;
        }

        private IEnumerable<(string Key, object Value)> DataMap()
        {
            var propertyData = Properties.Select(p => (p.Name, p.GetValue(this)));
            var allData = propertyData.Union(TrainingStats.Get(this)).Union(MaxStats.Get(this));
            return allData;
        }
    }
}