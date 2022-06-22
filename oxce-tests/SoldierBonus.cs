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
                TU: yaml.ParseIntOrZero("tu"),
                Stamina: yaml.ParseIntOrZero("stamina"),
                Health: yaml.ParseIntOrZero("health"),
                Bravery: yaml.ParseIntOrZero("bravery"),
                Reactions: yaml.ParseIntOrZero("reactions"),
                Firing: yaml.ParseIntOrZero("firing"),
                Throwing: yaml.ParseIntOrZero("throwing"),
                Strength: yaml.ParseIntOrZero("strength"),
                PsiStrength: yaml.ParseIntOrZero("psiStrength"),
                PsiSkill: yaml.ParseIntOrZero("psiSkill"),
                Melee: yaml.ParseIntOrZero("melee"),
                Mana: yaml.ParseIntOrZero("mana"));
    }
}