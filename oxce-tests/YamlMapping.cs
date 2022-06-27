using System;
using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

// Limited parser of a mapping from https://yaml.org/spec/1.2.2/
public class YamlMapping
{
    private const string Indent = "  ";
    private readonly IEnumerable<string> _lines;

    public YamlMapping(IEnumerable<string> lines)
    {
        _lines = lines.Where(line => !IsComment(line));
    }

    public IEnumerable<string> Lines(string key)
    {
        var outputLines = new List<string>();
        bool appendingLines = false;
            
        foreach (string line in _lines)
        {
            if (appendingLines)
            {
                if (FinishedAppendingLines(line))
                    break;

                AppendLine(line, outputLines);
            }
            else if (FoundKey(key, line))
            {
                appendingLines = true;

                AddValueFromKeyLineIfPresent(key, line, outputLines);
            }
        }

        // Remove indent
        var postProcessedOutputLines = outputLines.Select(line => line.Substring(Indent.Length));
        postProcessedOutputLines = postProcessedOutputLines.Select(TrimEndOfLineComment)
            .Where(line => line != string.Empty);
        return postProcessedOutputLines;
    }

    public IEnumerable<(string Key, string Value)> KeyValuePairs()
    {
        if (_lines.Count() == 1 && _lines.Single() == "{}")
            return Array.Empty<(string Key, string Value)>();

        return _lines.Select(
            line =>
            {
                var split = line.Split(": ");
                return (split[0], split[1]);
            });
    }

    private static bool FoundKey(string key, string line) => line.StartsWith(key + ":");

    private static bool FinishedAppendingLines(string line)
    {
        bool lineIsNotIndented = line.TrimStart() == line;
        return lineIsNotIndented;
    }

    public string ParseString(string key) => Lines(key).Single();

    public string ParseStringOrEmpty(string key)
    {
        var lines = Lines(key).ToList();
        return lines.Any() ? lines.Single() : string.Empty;
    }

    public int ParseInt(string key) => int.Parse(Lines(key).Single());

    public int ParseIntOrZero(string key)
        => int.TryParse(Lines(key).SingleOrDefault(), out var value) ? value : 0;

    public float ParseFloatOrZero(string key)
        => float.TryParse(Lines(key).SingleOrDefault(), out var value) ? value : 0;

    private static void AppendLine(string line, List<string> valueLines) => valueLines.Add(line);

    private static void AddValueFromKeyLineIfPresent(string key, string line, List<string> valueLines)
    {
        string lineWithKeyStripped = line.Substring((key + ":").Length).Trim();
        if (lineWithKeyStripped.Any())
            valueLines.Add(Indent + lineWithKeyStripped);
    }

    private bool IsComment(string line) => line.TrimStart().StartsWith("#");

    private static string TrimEndOfLineComment(string line)
        => line.Split('#').First().TrimEnd();
}