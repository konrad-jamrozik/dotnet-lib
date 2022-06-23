using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

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

    public static SoldierStats Sum(IEnumerable<SoldierStats> statsEnumerable)
    {
        var statsList = statsEnumerable.ToList();
        return new SoldierStats(
            TU: statsList.Sum(stats => stats.TU),
            Stamina: statsList.Sum(stats => stats.Stamina),
            Health: statsList.Sum(stats => stats.Health),
            Bravery: statsList.Sum(stats => stats.Bravery),
            Reactions: statsList.Sum(stats => stats.Reactions),
            Firing: statsList.Sum(stats => stats.Firing),
            Throwing: statsList.Sum(stats => stats.Throwing),
            Strength: statsList.Sum(stats => stats.Strength),
            PsiStrength: statsList.Sum(stats => stats.PsiStrength),
            PsiSkill: statsList.Sum(stats => stats.PsiSkill),
            Melee: statsList.Sum(stats => stats.Melee),
            Mana: statsList.Sum(stats => stats.Mana));
    }

    public SoldierStats SumWith(IEnumerable<SoldierStats> statsEnumerable)
        => Sum(this.WrapInList().Concat(statsEnumerable));

    public static IEnumerable<string> CsvHeaders() => new[]
    {
        "TU",
        "Stamina",
        "Health",
        "Bravery",
        "Reactions",
        "Firing",
        "Throwing",
        "Strength",
        "PsiStrength",
        "PsiSkill",
        "Melee",
        "Mana"
    };

    public IEnumerable<(string Name, object Value)> AsKeyValueTuples()
    {
        var propertyInfos = typeof(SoldierStats).GetProperties();
        return propertyInfos.Select(p => (p.Name, p.GetValue(this)));
    }
}