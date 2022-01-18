using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record Base(string Name, IEnumerable<Soldier> Soldiers, IEnumerable<ItemCount> ItemCounts)
{
    public static Base FromSaveFile(YamlMapping baseYaml)
    {
        var baseName = ParseString(baseYaml, "name");
        var soldiers = ParseBaseSoldiers(baseYaml, baseName);
        var itemCounts = ParseBaseItemCounts(baseYaml, baseName);
        return new Base(baseName, soldiers, itemCounts);
    }

    private static IEnumerable<Soldier> ParseBaseSoldiers(YamlMapping baseYaml, string baseName)
    {
        var soldiersYaml = new YamlBlockSequence(baseYaml.Lines("soldiers"));
        var soldiersLines = soldiersYaml.NodesLines();
        var soldiers = soldiersLines.Select(soldierLines => ParseSoldier(soldierLines, baseName));
        return soldiers;
    }

    private static Soldier ParseSoldier(IEnumerable<string> soldierLines, string baseName)
    {
        var soldier = new YamlMapping(soldierLines);
        var id = ParseInt(soldier, "id");
        var type = ParseString(soldier, "type");
        var name = ParseString(soldier, "name");
        var missions = ParseInt(soldier, "missions");
        var kills = ParseInt(soldier, "kills");
        var recovery = ParseFloatOrZero(soldier, "recovery");
        var manaMissing = ParseIntOrZero(soldier, "manaMissing");
        var rank = ParseInt(soldier, "rank");
        var soldierDiary = new YamlMapping(soldier.Lines("diary"));
        var monthsService = ParseIntOrZero(soldierDiary, "monthsService");
        var statGainTotal = ParseIntOrZero(soldierDiary, "statGainTotal");
        var initialStats = new YamlMapping(soldier.Lines("initialStats"));
        var currentStats = new YamlMapping(soldier.Lines("currentStats"));
        var currentTU = ParseInt(currentStats, "tu");
        var currentStamina = ParseInt(currentStats, "stamina");
        var currentHealth = ParseInt(currentStats, "health");
        var currentBravery = ParseInt(currentStats, "bravery");
        var currentReactions = ParseInt(currentStats, "reactions");
        var currentFiring = ParseInt(currentStats, "firing");
        var currentThrowing = ParseInt(currentStats, "throwing");
        var currentStrength = ParseInt(currentStats, "strength");
        var currentPsiStrength = ParseInt(currentStats, "psiStrength");
        var currentPsiSkill = ParseInt(currentStats, "psiSkill");
        var currentMelee = ParseInt(currentStats, "melee");
        var currentMana = ParseInt(currentStats, "mana");

        return new Soldier(
            id,
            name,
            type,
            baseName,
            missions,
            kills,
            rank,
            monthsService,
            recovery,
            manaMissing,
            statGainTotal,
            currentTU,
            currentStamina,
            currentHealth,
            currentBravery,
            currentReactions,
            currentFiring,
            currentThrowing,
            currentStrength,
            currentPsiStrength,
            currentPsiSkill,
            currentMelee,
            currentMana);
    }

    private static IEnumerable<ItemCount> ParseBaseItemCounts(
        YamlMapping baseYaml, string baseName)
    {
        var itemCountsYaml = new YamlMapping(baseYaml.Lines("items"));
        var itemCountsPairs = itemCountsYaml.KeyValuePairs();
        var itemCounts = itemCountsPairs.Select(
            pair => new ItemCount(baseName, pair.Key, int.Parse(pair.Value)));
        return itemCounts;
    }

    private static string ParseString(YamlMapping mapping, string key) 
        => mapping.Lines(key).Single();

    private static int ParseInt(YamlMapping mapping, string key)
        => int.Parse(mapping.Lines(key).Single());

    private static int ParseIntOrZero(YamlMapping mapping, string key)
        => int.TryParse(mapping.Lines(key).SingleOrDefault(), out var value) ? value : 0;

    private static float ParseFloatOrZero(YamlMapping mapping, string key)
        => float.TryParse(mapping.Lines(key).SingleOrDefault(), out var value) ? value : 0;
}