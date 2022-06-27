using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

internal record Craft(ItemCounts ItemCounts)
{
    public static Craft Parse(IEnumerable<string> craftLines, string baseName)
    {
        var craftYaml = new YamlMapping(craftLines);
        return new Craft(ItemCounts.Parse(craftYaml));
    }
}