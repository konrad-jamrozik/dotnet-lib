using System.Linq;
using System.Text.RegularExpressions;

namespace OxceTests
{
    public record Soldier(
        string Name,
        string Type,
        int Missions,
        string BaseName,
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
        public static string CsvHeaders() 
            => string.Join(",", typeof(Soldier).GetProperties().Select(p => p.Name));

        public string CsvString()
        {
            var str = ToString();
            var csvStr = Regex.Replace(str, "Soldier {(.*) }", "$1");
            csvStr = Regex.Replace(csvStr, " \\w+\\ = ", "");
            return csvStr;

        }
    }
}