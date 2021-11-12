using System.Linq;
using System.Text.RegularExpressions;

namespace OxceTests
{
    public record Soldier(
        string Type,
        string Name,
        int Missions,
        string BaseName,
        int Kills,
        int Rank,
        int MonthsService,
        int StatGainTotal,
        int CurrentPsiSkill)
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