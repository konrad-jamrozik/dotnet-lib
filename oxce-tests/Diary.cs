using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record Diary(IDictionary<string, int> Commendations)
{
    public static Diary Parse(IEnumerable<string> lines)
    {
        var diaryMapping = new YamlMapping(lines);
        var commendationsSeq = new YamlBlockSequence(diaryMapping.Lines("commendations"));
        var commendations = commendationsSeq
            .NodesLines()
            .Select(commendationLines => new YamlMapping(commendationLines))
            .ToDictionary(
                mapping =>
                {
                    var noun = mapping.ParseStringOrEmpty("noun");
                    var nounSuffix = noun != string.Empty ? "/" + noun : string.Empty;
                    return mapping.ParseString("commendationName") + nounSuffix;
                },
                mapping => mapping.ParseInt("decorationLevel"));

        return new Diary(commendations);
    }

    public int Decoration(string name)
        => Commendations.ContainsKey(name) ? Commendations[name] : 0;
}