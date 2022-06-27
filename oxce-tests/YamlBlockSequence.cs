using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

// Limited parser of a block sequence from https://yaml.org/spec/1.2.2/
public class YamlBlockSequence
{
    private const string Indent = "  ";
    private const string IndicatorPrefix = "- ";
    private readonly ParsedLines _parsedLines;
    private IEnumerable<string> LinesData => _parsedLines.Lines;

    public YamlBlockSequence(ParsedLines lines)
    {
        _parsedLines = lines with { Lines = lines.Lines.Where(line => !IsComment(line)).ToList() };
    }

    public YamlBlockSequence(IEnumerable<string> lines) : this(
        new ParsedLines(lines.ToList(), (0, lines.Count()))) { }

    public IEnumerable<IEnumerable<string>> NodesLines()
    {
        var nodesLines = new List<List<string>>();
        List<string> currentNodeLines = null;
        foreach (string line in LinesData)
        {
            if (FoundNextNode(line))
            {
                if (currentNodeLines != null)
                    nodesLines.Add(currentNodeLines);
                currentNodeLines = new List<string>();
                AddNodeLineFromIndicatorLineIfPresent(line, currentNodeLines);
            }
            else
            {
                AddNodeLine(line, currentNodeLines);
            }
        }

        if (currentNodeLines != null) 
            nodesLines.Add(currentNodeLines);

        var indentedNodesLines = nodesLines.Select(
            nodeLines => nodeLines.Select(nodeLine => nodeLine.Substring(Indent.Length)));
        return indentedNodesLines;
    }

    private void AddNodeLine(string line, List<string> currentNodeLines) 
        => currentNodeLines.Add(line);

    private bool FoundNextNode(string line) 
        => line.StartsWith(IndicatorPrefix) || line == IndicatorPrefix.Trim();


    private static void AddNodeLineFromIndicatorLineIfPresent(string line, List<string> nodeLines)
    {
        if (line == IndicatorPrefix.Trim())
            return;

        string lineWithIndicatorStripped = line.Substring(IndicatorPrefix.Length).Trim();
        if (lineWithIndicatorStripped.Any())
            nodeLines.Add(Indent + lineWithIndicatorStripped);
    }

    private bool IsComment(string line) => line.TrimStart().StartsWith("#");
}