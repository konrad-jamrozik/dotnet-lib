using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedMember.Global
// Reason: all properties are accessed via the "Properties" member

namespace OxceTests;

public record Soldier(
    int Id,
    string Name,
    string Type,
    string BaseName,
    bool InTransfer,
    int Missions,
    int Kills,
    int Rank,
    int MonthsService,
    float Recovery,
    int ManaMissing,
    int StatGainTotal,
    bool CombatPilot,
    bool GunKata,
    bool Dagon,
    bool MartialArts,
    bool BioEnOrLegacy,
    bool Tni,
    bool Helix,
    int CurrentTU,
    int CurrentStamina,
    int CurrentHealth,
    int CurrentBravery,
    int CurrentReactions,
    int CurrentFiring,
    int CurrentThrowing,
    int CurrentStrength,
    int CurrentPsiStrength,
    int CurrentPsiSkill,
    int CurrentMelee,
    int CurrentMana,
    // Order of fields from commendations ruleset file.
    int Monster,
    int Wrestler,
    int Slasher,
    int Rocket,
    int Trooper,
    int Sniper,
    int Shotgun,
    int Gunslinger,
    int Assaulter,
    int Cannoneer,
    int Bombardier,
    int Warrior,
    int Technician,
    int Traditionalist,
    int Incapacitator,
    int JungleMower,
    int Gunner,
    int Purifier,
    int Grenadier,
    int Tasemaster,
    int Suppressor,
    int Sorcerer,
    Diary Diary)
{
    private static IEnumerable<PropertyInfo> Properties { get; } =
        typeof(Soldier).GetProperties().Where(pi => pi.Name != nameof(Diary));

    public static string CsvHeaders()
        => string.Join(",", 
            Properties.Select(p => p.Name)
                .Union(TrainingStats.CsvHeaders()) // kja should be Concat instead?
                .Union(MaxStats.CsvHeaders())); // kja should be Concat instead?

    public string CsvString()
    {
        var str = ToString();
        var csvStr = Regex.Replace(str, $"{nameof(Soldier)} {{(.*) }}", "$1");
        csvStr = Regex.Replace(csvStr, " \\w+\\ = ", "");
        return csvStr;
    }

    public override string ToString()
        => $"{nameof(Soldier)} {{ "
           + string.Join(", ", DataMap().Select(p => $"{p.Key} = {p.Value}")) + " }";

    private IEnumerable<(string Key, object Value)> DataMap()
    {
        var propertyData = Properties.Select(p => (p.Name, p.GetValue(this)));
        var allData = propertyData
            .Union(TrainingStats.Get(this)) // kja should be Concat instead?
            .Union(MaxStats.Get(this)); // kja should be Concat instead?
        return allData;
    }

    /// <remarks>
    /// We do not measure kills per mission for soldiers with few missions,
    /// as it is too luck dependent and would scramble sorting order.
    /// </remarks> 
    public float KillsPerMission => Missions >= 5 ? (float) Kills / Missions : 0;

    /// <summary>
    /// StatGainTotal but without the psi skill of the soldier, as psi skill
    /// training mode and magnitude is different, and would skew comparisons.
    /// </summary>
    public int StatGainReal => Math.Max(StatGainTotal - CurrentPsiSkill, 0);

    public string HelixName => Name.EndsWith("H") ? "TRUE" : "FALSE";

    public string Humanoid => Type is "STR_SOLDIER" or "STR_HYBRID" ? "TRUE" : "FALSE";

    public static Soldier Parse(
        IEnumerable<string> soldierLines,
        string baseName,
        bool inTransfer)
    {
        var soldier = new YamlMapping(soldierLines);
        var id = soldier.ParseInt("id");
        var type = soldier.ParseString("type");
        var name = soldier.ParseString("name");
        var missions = soldier.ParseInt("missions");
        var kills = soldier.ParseInt("kills");
        var recovery = soldier.ParseFloatOrZero("recovery");
        var manaMissing = soldier.ParseIntOrZero("manaMissing");
        var rank = soldier.ParseInt("rank");
        var soldierDiary = new YamlMapping(soldier.Lines("diary"));
        var monthsService = soldierDiary.ParseIntOrZero("monthsService");
        var statGainTotal = soldierDiary.ParseIntOrZero("statGainTotal");
        var initialStats = new YamlMapping(soldier.Lines("initialStats"));
        var currentStats = new YamlMapping(soldier.Lines("currentStats"));
        var transformationBonuses =
            TransformationBonuses.Parse(soldier.Lines("transformationBonuses"));
        var currentTU = currentStats.ParseInt("tu");
        var currentStamina = currentStats.ParseInt("stamina");
        var currentHealth = currentStats.ParseInt("health");
        var currentBravery = currentStats.ParseInt("bravery");
        var currentReactions = currentStats.ParseInt("reactions");
        var currentFiring = currentStats.ParseInt("firing");
        var currentThrowing = currentStats.ParseInt("throwing");
        var currentStrength = currentStats.ParseInt("strength");
        var currentPsiStrength = currentStats.ParseInt("psiStrength");
        var currentPsiSkill = currentStats.ParseInt("psiSkill");
        var currentMelee = currentStats.ParseInt("melee");
        var currentMana = currentStats.ParseInt("mana");
        var diary = Diary.Parse(soldier.Lines("diary"));

        return new Soldier(
            id,
            name,
            type,
            baseName,
            inTransfer,
            missions,
            kills,
            rank,
            monthsService,
            recovery,
            manaMissing,
            statGainTotal,
            transformationBonuses.Contains("STR_COMBAT_PILOT_TRAINING"),
            transformationBonuses.Contains("STR_GUN_KATA"),
            transformationBonuses.Contains("STR_DAGONIZATION"),
            transformationBonuses.Contains("STR_MARTIAL_ARTS_TRAINING"),
            transformationBonuses.Contains("STR_BIO_ENHANCEMENT") || transformationBonuses.Contains("STR_SECTOID_LEGACY"),
            transformationBonuses.Contains("STR_TACTICAL_NEURAL_IMPLANT"),
            transformationBonuses.Contains("STR_HELIX_KNIGHT") || transformationBonuses.Contains("STR_HELIX_PSION"),
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
            currentMana,
            diary.Decoration("STR_MEDAL_MONSTER_NAME"),
            diary.Decoration("STR_MEDAL_WRESTLER_NAME"),
            diary.Decoration("STR_MEDAL_SLASHER_NAME"),
            diary.Decoration("STR_MEDAL_ROCKET_SCIENTIST_NAME"),
            diary.Decoration("STR_MEDAL_TROOPER_NAME"),
            diary.Decoration("STR_MEDAL_SNIPER_NAME"),
            diary.Decoration("STR_MEDAL_SHOTGUN_SURGEON_NAME"),
            diary.Decoration("STR_MEDAL_GUNSLINGER_NAME"),
            diary.Decoration("STR_MEDAL_ASSAULTER_NAME"),
            diary.Decoration("STR_MEDAL_CANNONEER_NAME"),
            diary.Decoration("STR_MEDAL_BOMBARDIER_NAME"),
            diary.Decoration("STR_MEDAL_WARRIOR_NAME"),
            diary.Decoration("STR_MEDAL_TECHNICIAN_NAME"),
            diary.Decoration("STR_MEDAL_TRADITIONALIST_NAME"),
            diary.Decoration("STR_MEDAL_INCAPACITATOR_NAME"),
            diary.Decoration("STR_MEDAL_JUNGLE_MOWER_NAME"),
            diary.Decoration("STR_MEDAL_GUNNER_NAME"),
            diary.Decoration("STR_MEDAL_PURIFIER_NAME"),
            diary.Decoration("STR_MEDAL_GRENADIER_NAME"),
            diary.Decoration("STR_MEDAL_TASEMASTER_NAME"),
            diary.Decoration("STR_MEDAL_SUPPRESSOR_NAME"),
            diary.Decoration("STR_MEDAL_SORCERER_NAME"),
            diary);
    }

}