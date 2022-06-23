namespace OxceTests;

public record SoldierStats(
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
    public static SoldierStats FromStatsYaml(YamlMapping yaml)
        => new SoldierStats(
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