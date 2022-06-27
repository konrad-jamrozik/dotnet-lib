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
    string CraftName,
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
    SoldierStats CurrentStats,
    SoldierWeaponClassDecorations WeaponClassDecorations,
    Diary Diary,
    TransformationBonuses TransformationBonuses)
{
    private static readonly string[] PropertiesExcludedFromPrinting =
    {
        nameof(CurrentStats), 
        nameof(WeaponClassDecorations), 
        nameof(Diary),
        nameof(TransformationBonuses),
        nameof(Craft)
    };

    private static IEnumerable<PropertyInfo> Properties { get; } =
        typeof(Soldier).GetProperties()
            .Where(pi => !PropertiesExcludedFromPrinting.Contains(pi.Name));

    public static string CsvHeaders()
        => string.Join(
            ",",
            Properties.Select(p => p.Name)
                .Concat(SoldierStats.CsvHeaders())
                .Concat(TrainingStats.CsvHeaders())
                .Concat(MaxStats.CsvHeaders())
                .Concat(SoldierWeaponClassDecorations.CsvHeaders()));

    public string CsvString(CommendationBonuses commendationBonuses)
    {
        var str = ToString(commendationBonuses);
        var csvStr = Regex.Replace(str, $"{nameof(Soldier)} {{(.*) }}", "$1");
        csvStr = Regex.Replace(csvStr, " \\w+\\ = ", "");
        return csvStr;
    }

    public string ToString(CommendationBonuses commendationBonuses)
        => $"{nameof(Soldier)} {{ "
           + string.Join(", ", DataMap(commendationBonuses).Select(p => $"{p.Key} = {p.Value}")) + " }";

    private IEnumerable<(string Key, object Value)> DataMap(CommendationBonuses commendationBonuses)
    {
        var propertyData = Properties.Select(p => (p.Name, p.GetValue(this)));
        var allData = propertyData
            .Concat(commendationBonuses.StatsWithBonuses(this).AsKeyValueTuples())
            .Concat(TrainingStats.Get(this))
            .Concat(MaxStats.Get(this))
            .Concat(WeaponClassDecorations.AsKeyValueTuples());
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
    public int StatGainReal => Math.Max(StatGainTotal - CurrentStats.PsiSkill, 0);

    public string HelixName => Name.EndsWith("H") ? "TRUE" : "FALSE";

    public string Humanoid => Type is "STR_SOLDIER" or "STR_HYBRID" ? "TRUE" : "FALSE";

    public static Soldier Parse(
        IEnumerable<string> soldierLines,
        string baseName,
        bool inTransfer)
    {
        var soldierYaml = new YamlMapping(soldierLines);
        var id = soldierYaml.ParseInt("id");
        var type = soldierYaml.ParseString("type");
        var name = soldierYaml.ParseString("name");
        var missions = soldierYaml.ParseInt("missions");
        var kills = soldierYaml.ParseInt("kills");
        var recovery = soldierYaml.ParseFloatOrZero("recovery");
        var manaMissing = soldierYaml.ParseIntOrZero("manaMissing");
        var rank = soldierYaml.ParseInt("rank");
        var soldierDiary = new YamlMapping(soldierYaml.Lines("diary"));
        var monthsService = soldierDiary.ParseIntOrZero("monthsService");
        var statGainTotal = soldierDiary.ParseIntOrZero("statGainTotal");
        var initialStats = new YamlMapping(soldierYaml.Lines("initialStats"));
        var currentStatsYaml = new YamlMapping(soldierYaml.Lines("currentStats"));
        var currentStats = SoldierStats.FromStatsYaml(currentStatsYaml);
        var diary = Diary.Parse(soldierYaml.Lines("diary"));
        var weaponClassDecorations = SoldierWeaponClassDecorations.FromDiary(diary);
        var previousTransformations =
            PreviousTransformations.Parse(soldierYaml.Lines("previousTransformations"));
        var transformationBonuses =
            TransformationBonuses.Parse(soldierYaml.Lines("transformationBonuses"));

        return new Soldier(
            id,
            name,
            type,
            baseName,
            ParseCraftName(baseName, soldierYaml),
            inTransfer,
            missions,
            kills,
            rank,
            monthsService,
            recovery,
            manaMissing,
            statGainTotal,
            transformationBonuses.Contains("STR_COMBAT_PILOT_TRAINING") ||
            // Need to look also into previousTransformations for combat pilot training because 
            // this transformation doesn't have entry for 'soldierBonusType'.
            previousTransformations.Contains("STR_COMBAT_PILOT_TRAINING"),
            transformationBonuses.Contains("STR_GUN_KATA"),
            transformationBonuses.Contains("STR_DAGONIZATION"),
            transformationBonuses.Contains("STR_MARTIAL_ARTS_TRAINING"),
            transformationBonuses.Contains("STR_BIO_ENHANCEMENT") || transformationBonuses.Contains("STR_SECTOID_LEGACY"),
            transformationBonuses.Contains("STR_TACTICAL_NEURAL_IMPLANT"),
            transformationBonuses.Contains("STR_HELIX_KNIGHT") || transformationBonuses.Contains("STR_HELIX_PSION"),
            currentStats,
            weaponClassDecorations,
            diary,
            transformationBonuses);
    }

    private static string ParseCraftName(string baseName, YamlMapping soldierYaml)
    {
        var craft = Craft.Parse(soldierYaml.Lines("craft"), baseName);
        var craftName = craft != null ? craft.Type + "/" + craft.Id : "";
        return craftName;
    }
}