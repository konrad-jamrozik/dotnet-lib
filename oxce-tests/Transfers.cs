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
}