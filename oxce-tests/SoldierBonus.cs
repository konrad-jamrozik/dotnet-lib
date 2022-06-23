namespace OxceTests;

public record SoldierBonus(string Name, SoldierStats Stats)
{
    public static SoldierBonus FromSoldierYaml(YamlMapping yaml)
    {
        var name = yaml.ParseString("name");
        var stats = SoldierStats.FromStatsYaml(new YamlMapping(yaml.Lines("stats")));

        return new SoldierBonus(name, stats);
    }
}