using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record TransformationBonuses(ISet<string> TransformationNames)
{
    public static TransformationBonuses Parse(IEnumerable<string> lines)
    {
        var transformationNames =
            new YamlMapping(lines).KeyValuePairs().Select(kvp => kvp.Key).ToHashSet();
        return new TransformationBonuses(transformationNames);
    }
}