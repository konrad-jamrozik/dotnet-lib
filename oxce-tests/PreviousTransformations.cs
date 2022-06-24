using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record PreviousTransformations(ISet<string> TransformationNames)
{
    public static PreviousTransformations Parse(IEnumerable<string> lines)
    {
        var transformationNames =
            new YamlMapping(lines).KeyValuePairs().Select(kvp => kvp.Key).ToHashSet();
        return new PreviousTransformations(transformationNames);
    }

    public bool Contains(string transformationName)
        => TransformationNames.Contains(transformationName);
}