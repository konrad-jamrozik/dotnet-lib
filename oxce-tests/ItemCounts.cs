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

    public static ItemCounts Merge(
        IEnumerable<ItemCounts> itemCountsEnumerable)
    {
        // kja I need here an abstraction: dict1.Merge(dict2, value => value.Sum())
        // when done, reuse the ToDictionary proposed in transfers.ItemCountsMap (above)
        // as well as when computing itemCountsMap (even higher above).
        var mergedItemCountsMap = itemCountsEnumerable
            .SelectMany(itemCounts => itemCounts.Map.Select(kvp => kvp))
            .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
            .ToDictionary(
                itemIdCounts => itemIdCounts.Key,
                itemIdCounts => itemIdCounts.Sum());
        return new ItemCounts(mergedItemCountsMap);
    }

    public IEnumerable<ItemCount> ToItemCountEnumerable(string baseName)
        => Map.Select(
            itemCount => new ItemCount(baseName, itemCount.Key, itemCount.Value));
}