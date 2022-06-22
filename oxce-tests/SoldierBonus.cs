namespace OxceTests;

public record SoldierBonus(string Name, SoldierBonus.StatsData Stats)
{
    public static SoldierBonus FromSoldierYaml(YamlMapping yaml)
    {
        var name = yaml.ParseString("name");
        var stats = StatsData.FromStatsYaml(new YamlMapping(yaml.Lines("stats")));

        return new SoldierBonus(name, stats);
    }

    public record StatsData(
        int TU,
        int Stamina,
        int Health,
        int Bravery,
        int Reactions,
        int Firing,
        int Throwing,
        int Strength,
        int PsiStrength,
        int PsiSkill,
        int Melee,
        int Mana)
    {
        public static StatsData FromStatsYaml(YamlMapping yaml)
            => new StatsData(
                TU: yaml.ParseIntOrZero("TU"),
                Stamina: yaml.ParseIntOrZero("Stamina"),
                Health: yaml.ParseIntOrZero("Health"),
                Bravery: yaml.ParseIntOrZero("Bravery"),
                Reactions: yaml.ParseIntOrZero("Reactions"),
                Firing: yaml.ParseIntOrZero("Firing"),
                Throwing: yaml.ParseIntOrZero("Throwing"),
                Strength: yaml.ParseIntOrZero("Strength"),
                PsiStrength: yaml.ParseIntOrZero("PsiStrength"),
                PsiSkill: yaml.ParseIntOrZero("PsiSkill"),
                Melee: yaml.ParseIntOrZero("Melee"),
                Mana: yaml.ParseIntOrZero("Mana"));
    }
}