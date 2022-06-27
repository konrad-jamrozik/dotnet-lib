using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record Base(string Name, IEnumerable<Soldier> Soldiers, IEnumerable<ItemCount> ItemCounts)
{
    public static Base FromBaseYaml(YamlMapping yaml)
    {
        var baseName = yaml.ParseString("name");
        var transfers = Transfers.FromBaseYaml(yaml, baseName);
        var soldiers = ParseBaseSoldiers(yaml, baseName, transfers);
        var crafts = ParseCrafts(yaml, baseName);
        var itemCounts = ParseBaseItemCounts(yaml, baseName, transfers, crafts);
        return new Base(baseName, soldiers, itemCounts);
    }

    private static IEnumerable<Soldier> ParseBaseSoldiers(
        YamlMapping baseYaml,
        string baseName,
        Transfers transfers)
    {
        var soldiersYaml = new YamlBlockSequence(baseYaml.Lines("soldiers"));
        var soldiersNodesLines = soldiersYaml.NodesLines();
        var soldiers = soldiersNodesLines.Select(
            soldierLines => Soldier.Parse(soldierLines, baseName, inTransfer: false));
        
        return soldiers.Concat(transfers.Soldiers).OrderBy(soldier => soldier.Id);
    }

    
    private static IEnumerable<Craft> ParseCrafts(YamlMapping baseYaml, string baseName)
    {
        var craftsYaml = new YamlBlockSequence(baseYaml.Lines("crafts"));
        var craftsNodesLines = craftsYaml.NodesLines();
        var crafts = craftsNodesLines.Select(
            craftLines => Craft.Parse(craftLines, baseName));

        return crafts;
    }

    private static IEnumerable<ItemCount> ParseBaseItemCounts(
        YamlMapping baseYaml, string baseName, Transfers transfers, IEnumerable<Craft> crafts)
    {
        var baseItemCounts = OxceTests.ItemCounts.Parse(baseYaml);
        var mergedItemCounts = OxceTests.ItemCounts.Merge(
            new List<ItemCounts> { baseItemCounts, transfers.ItemCounts }
                .Concat(crafts.Select(craft => craft.ItemCounts)));

        return mergedItemCounts.ToItemCountEnumerable(baseName);
    }
}