using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record Craft(int Id, string Type, (string Lon, string Lat) Coords, ItemCounts ItemCounts)
{
    public static Craft Parse(IEnumerable<string> craftLines, string baseName)
    {
        if (!craftLines.Any())
            return null;

        var craftYaml = new YamlMapping(craftLines);
        
        return new Craft(
            craftYaml.ParseInt("id"),
            craftYaml.ParseString("type"),
            (craftYaml.ParseString("lon"), craftYaml.ParseString("lat")),
            ItemCounts.Parse(craftYaml));
    }
}