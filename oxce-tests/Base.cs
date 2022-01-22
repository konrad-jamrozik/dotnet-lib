using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record Base(string Name, IEnumerable<Soldier> Soldiers, IEnumerable<ItemCount> ItemCounts)
{
    public static Base FromSaveFile(YamlMapping baseYaml)
    {
        var baseName = baseYaml.ParseString("name");
        var soldiers = ParseBaseSoldiers(baseYaml, baseName);
        var itemCounts = ParseBaseItemCounts(baseYaml, baseName);
        return new Base(baseName, soldiers, itemCounts);
    }

    private static IEnumerable<Soldier> ParseBaseSoldiers(YamlMapping baseYaml, string baseName)
    {
        var soldiersYaml = new YamlBlockSequence(baseYaml.Lines("soldiers"));
        var soldiersLines = soldiersYaml.NodesLines();
        var soldiers = soldiersLines.Select(
            soldierLines => Soldier.Parse(soldierLines, baseName, inTransfer: false));

        var transfers = Transfers.FromBaseYaml(baseYaml, baseName);
        return soldiers.Union(transfers.Soldiers).OrderBy(soldier => soldier.Id);
    }

    private static IEnumerable<ItemCount> ParseBaseItemCounts(
        YamlMapping baseYaml, string baseName)
    {
        var itemCountsYaml = new YamlMapping(baseYaml.Lines("items"));
        var itemCountsPairs = itemCountsYaml.KeyValuePairs();
        var itemCounts = itemCountsPairs.Select(
            pair => new ItemCount(baseName, pair.Key, int.Parse(pair.Value)));
        return itemCounts;
    }

}