using System.Collections.Generic;
using System.Linq;

namespace OxceTests
{


    public class YamlMapping
    {
        private readonly IEnumerable<string> _lines;

        public YamlMapping(IEnumerable<string> lines)
        {
            _lines = lines;
        }

        public IEnumerable<string> Lines(string key)
        {
            List<string> valueLines = new List<string>();
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

            var indentedValueLines = valueLines.Select(line => line.Substring(2));
            return indentedValueLines;
        }

        private static bool FoundKey(string key, string line)
        {
            return line.StartsWith(key + ":");
        }

        private static bool FinishedAppendingLines(string line)
        {
            bool endOfIndentedLines = line.TrimStart() != line;

            if (endOfIndentedLines)
                return true;
            return false;
        }

        private static void AppendLine(List<string> valueLines, string line)
        {
            valueLines.Add(line);
        }

        private static void AddValueFromKeyLineIfPresent(string key, string line, List<string> valueLines)
        {
            string keyLine = line.Substring((key + ":").Length).Trim();
            if (keyLine.Any())
                valueLines.Add(keyLine);
        }
    }
}