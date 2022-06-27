using System;
using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

// Limited parser of a mapping from https://yaml.org/spec/1.2.2/
public class YamlMapping
{
    private const string Indent = "  ";
    private readonly ParsedLines _parsedLines;

    private IEnumerable<string> LinesData => _parsedLines.Lines;

    public YamlMapping(ParsedLines lines)
    {
        _parsedLines = lines with { Lines = lines.Lines.Where(line => !IsComment(line)).ToList() };
    }

    public YamlMapping(IEnumerable<string> lines) : this(new ParsedLines(lines.ToList(), (0, lines.Count())))
    {
    }

    public IEnumerable<string> Lines(string key)
        => LinesInternal(key).Lines;

    private ParsedLines LinesInternal(string key)
    {
        int startLineOffset = _parsedLines.Offsets.start;
        int endLineOffset = _parsedLines.Offsets.start;
        var outputLines = new List<string>();
        bool appendingLines = false;

        foreach (string line in _parsedLines.Lines)
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
            else
            {
                // We increase the start line offset on each line
                // only if we didn't find the key yet
                startLineOffset++;
            }

            endLineOffset++;
        }

        // Remove indent
        var postProcessedOutputLines = outputLines.Select(line => line.Substring(Indent.Length));

        postProcessedOutputLines = postProcessedOutputLines.Select(TrimEndOfLineComment)
            .Where(line => line != string.Empty);

        return new ParsedLines(postProcessedOutputLines.ToList(), (startLineOffset, endLineOffset));
    }

    public IEnumerable<(string Key, string Value)> KeyValuePairs()
    {
        if (LinesData.Count() == 1 && LinesData.Single() == "{}")
            return Array.Empty<(string Key, string Value)>();

        return LinesData.Select(
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