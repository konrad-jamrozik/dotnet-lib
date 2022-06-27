using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record Transfers(YamlBlockSequence TransfersYaml, string BaseName)
{
    public static Transfers FromBaseYaml(YamlMapping baseYaml, string baseName)
    {
        var transfersYaml = new YamlBlockSequence(baseYaml.Lines("transfers"));
        return new Transfers(transfersYaml, baseName);
    }

    public IEnumerable<Soldier> Soldiers =>
        TransfersYaml.NodesLines()
            .Select(transferLines => new YamlMapping(transferLines))
            .Select(transferYaml => transferYaml.Lines("soldier").ToList())
            .Where(soldierLines => soldierLines.Any())
            .Select(soldierLines => Soldier.Parse(soldierLines, BaseName, inTransfer: true));

    public ItemCounts ItemCounts
    {
        get
        {
            var lines = TransfersYaml.NodesLines();

            var itemTransfersMappings = lines
                .Select(transferLines => new YamlMapping(transferLines))
                .Where(mapping => mapping.Lines("itemId").Any());

            // kja I need here an abstraction: enumerable.ToDictionary(
            //  mapping => mapping.ParseString("itemId"),
            //  mapping => mapping.ParseInt("itemQty"),
            //  values => values.Sum()) // duplicates merge func
            var itemCountsMap = itemTransfersMappings
                .GroupBy(
                    mapping => mapping.ParseString("itemId"),
                    mapping => mapping.ParseInt("itemQty"))
                .ToDictionary(
                    itemIdQts => itemIdQts.Key,
                    itemIdQts => itemIdQts.Sum());

            return new ItemCounts(itemCountsMap);
        }
    }
}