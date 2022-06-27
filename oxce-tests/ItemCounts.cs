using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record ItemCounts(Dictionary<string, int> Map)
{
    public static ItemCounts Parse(YamlMapping yaml)
    {
        var itemCountsYaml = new YamlMapping(yaml.Lines("items"));
        var itemCountEntries = itemCountsYaml.KeyValuePairs();

        Dictionary<string, int> itemCountsMap = itemCountEntries
            .ToDictionary(kvp => kvp.Key, kvp => int.Parse((string)kvp.Value));

        return new ItemCounts(itemCountsMap);
    }
}