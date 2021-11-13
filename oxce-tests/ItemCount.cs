using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OxceTests
{
    public record ItemCount(string BaseName, string Name, int Count)
    {
        public static string CsvHeaders()
            => string.Join(",", Properties.Select(p => p.Name));

        private static IEnumerable<PropertyInfo> Properties { get; } = typeof(ItemCount).GetProperties();

        public override string ToString() => $"{nameof(ItemCount)} {{ " 
                                             + string.Join(", ", DataMap().Select(p => $"{p.Key} = {p.Value}")) + " }";

        public string CsvString()
        {
            var str = ToString();
            var csvStr = Regex.Replace(str, $"{nameof(ItemCount)} {{(.*) }}", "$1");
            csvStr = Regex.Replace(csvStr, " \\w+\\ = ", "");
            return csvStr;
        }

        private IEnumerable<(string Key, object Value)> DataMap()
        {
            var propertyData = Properties.Select(p => (p.Name, p.GetValue(this)));
            return propertyData;
        }
    }
}