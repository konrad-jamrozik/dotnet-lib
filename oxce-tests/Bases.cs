using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record Bases(IEnumerable<Base> BaseData) : IEnumerable<Base>
{
    public static Bases FromSaveGameYamlMapping(YamlMapping yamlMapping)
    {
        var basesLines = yamlMapping.Lines("bases").ToList();
        var basesNodesLines = new YamlBlockSequence(basesLines).NodesLines();
        var bases = basesNodesLines.Select(Base.FromSaveGameLines);
        return new Bases(bases);
    }

    public IEnumerable<Soldier> Soldiers => BaseData.SelectMany(@base => @base.Soldiers);

    public IEnumerable<ItemCount> ItemCounts => BaseData.SelectMany(@base => @base.ItemCounts);

    public IEnumerator<Base> GetEnumerator() => BaseData.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}