using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace OxceTests;

public record Bases(IEnumerable<Base> BaseData) : IEnumerable<Base>
{
    public IEnumerable<Soldier> Soldiers => BaseData.SelectMany(@base => @base.Soldiers);

    public IEnumerable<ItemCount> ItemCounts => BaseData.SelectMany(@base => @base.ItemCounts);

    public IEnumerator<Base> GetEnumerator() => BaseData.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static Bases FromSaveFile(YamlMapping saveGameYaml)
    {
        var basesLines = saveGameYaml.Lines("bases").ToList();
        var basesNodesLines = new YamlBlockSequence(basesLines).NodesLines();
        var bases = basesNodesLines.Select(lines => Base.FromSaveFile(new YamlMapping(lines)));
        return new Bases(bases);
    }

    public async Task WriteSoldiers(
        IFileSystem fs,
        string soldiersOutputPath)
    {
        string[] csvLines = Soldier.CsvHeaders().InList()
            .Concat(Soldiers.OrderBy(s => s.Id).Select(s => s.CsvString())).ToArray();

        await fs.WriteAllLinesAsync(soldiersOutputPath, csvLines);

        await Console.Out.WriteLineAsync("Wrote bases soldiers data to " + soldiersOutputPath);
    }

    public async Task WriteItemCounts(
        IFileSystem fs,
        string itemCountsOutputPath)
    {
        string[] csvLines = ItemCount.CsvHeaders().InList()
            .Concat(ItemCounts.Select(s => s.CsvString())).ToArray();

        await fs.WriteAllLinesAsync(itemCountsOutputPath, csvLines);

        await Console.Out.WriteLineAsync("Wrote bases item count data to " + itemCountsOutputPath);
    }
}