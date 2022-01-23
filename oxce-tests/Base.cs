using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record Base(string Name, IEnumerable<Soldier> Soldiers, IEnumerable<ItemCount> ItemCounts)
{
    public static Base FromSaveFile(YamlMapping baseYaml)
    {
        var baseName = baseYaml.ParseString("name");
        var transfers = Transfers.FromBaseYaml(baseYaml, baseName);
        var soldiers = ParseBaseSoldiers(baseYaml, baseName, transfers);
        var itemCounts = ParseBaseItemCounts(baseYaml, baseName, transfers);
        return new Base(baseName, soldiers, itemCounts);
    }

    private static IEnumerable<Soldier> ParseBaseSoldiers(
        YamlMapping baseYaml,
        string baseName,
        Transfers transfers)
    {
        var soldiersYaml = new YamlBlockSequence(baseYaml.Lines("soldiers"));
        var soldiersLines = soldiersYaml.NodesLines();
        var soldiers = soldiersLines.Select(
            soldierLines => Soldier.Parse(soldierLines, baseName, inTransfer: false));
        
        return soldiers.Union(transfers.Soldiers).OrderBy(soldier => soldier.Id);
    }

    private static IEnumerable<ItemCount> ParseBaseItemCounts(
        YamlMapping baseYaml, string baseName, Transfers transfers)
    {
        var itemCountsYaml = new YamlMapping(baseYaml.Lines("items"));
        var itemCountEntries = itemCountsYaml.KeyValuePairs();

        Dictionary<string, int> itemCountsMap = itemCountEntries
            .GroupBy(entry => entry.Key, entry => int.Parse(entry.Value))
            .ToDictionary(itemIdCounts => itemIdCounts.Key, itemIdCounts => itemIdCounts.Sum());

        var transferredItemCountsMap = transfers.ItemCountsMap;

        // kja I need here an abstraction: dict1.Merge(dict2, value => value.Sum())
        // when done, reuse the ToDictionary proposed in transfers.ItemCountsMap (above)
        // as well as when computing itemCountsMap (even higher above).
        var combinedItemCountsMap = itemCountsMap.Select(kvp => kvp)
            .Union(transferredItemCountsMap.Select(kvp => kvp))
            .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
            .ToDictionary(
                itemIdCounts => itemIdCounts.Key,
                itemIdCounts => itemIdCounts.Sum());

        return combinedItemCountsMap.Select(
            itemCount => new ItemCount(baseName, itemCount.Key, itemCount.Value));
    }

}