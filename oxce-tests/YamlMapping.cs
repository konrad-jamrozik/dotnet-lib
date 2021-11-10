using System.Collections.Generic;
using System.Linq;

namespace OxceTests
{
    // Partial parser Mapping from https://yaml.org/spec/1.2.2/
    public class YamlMapping
    {
        private const string Indent = "  ";
        private readonly IEnumerable<string> _lines;

        public YamlMapping(IEnumerable<string> lines)
        {
            _lines = lines;
        }

        public IEnumerable<string> Lines(string key)
        {
            var valueLines = new List<string>();
            bool appendingLines = false;
            
            foreach (string line in _lines)
            {
                if (appendingLines)
                {
                    if (FinishedAppendingLines(line)) 
                        break;

                    AppendLine(valueLines, line);
                } 
                else if (FoundKey(key, line))
                {
                    appendingLines = true;

                    AddValueFromKeyLineIfPresent(key, line, valueLines);
                }
            }

            var indentedValueLines = valueLines.Select(line => line.Substring(Indent.Length));
            return indentedValueLines;
        }

        private static bool FoundKey(string key, string line) => line.StartsWith(key + ":");

        private static bool FinishedAppendingLines(string line)
        {
            bool lineIsNotIndented = line.TrimStart() == line;
            return lineIsNotIndented;
        }

        private static void AppendLine(List<string> valueLines, string line) => valueLines.Add(line);

        private static void AddValueFromKeyLineIfPresent(string key, string line, List<string> valueLines)
        {
            string lineWithKeyStripped = line.Substring((key + ":").Length).Trim();
            if (lineWithKeyStripped.Any())
                valueLines.Add(Indent + lineWithKeyStripped);
        }
    }
}