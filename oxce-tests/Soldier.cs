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
        nameof(TransformationBonuses)
    };

    private static IEnumerable<PropertyInfo> Properties { get; } =
        typeof(Soldier).GetProperties()
            .Where(pi => !PropertiesExcludedFromPrinting.Contains(pi.Name));

    public static string CsvHeaders()
        => string.Join(
            ",",
            Properties.Select(p => p.Name)
                .Concat(SoldierStats.CsvHeaders().Select(header => "Current" + header))
                .Concat(TrainingStats.CsvHeaders())
                .Concat(MaxStats.CsvHeaders())
                .Concat(SoldierWeaponClassDecorations.CsvHeaders()));

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
            .Concat(CurrentStats.AsKeyValueTuples())
            // KJA need to ensure that CurrentStats aren't augmented by commendations
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
        var currentStatsYaml = new YamlMapping(soldier.Lines("currentStats"));
        var currentStats = SoldierStats.FromStatsYaml(currentStatsYaml);
        var diary = Diary.Parse(soldier.Lines("diary"));
        var weaponClassDecorations = SoldierWeaponClassDecorations.FromDiary(diary);
        var transformationBonuses =
            TransformationBonuses.Parse(soldier.Lines("transformationBonuses"));

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
            currentStats,
            weaponClassDecorations,
            diary,
            transformationBonuses);
    }

}