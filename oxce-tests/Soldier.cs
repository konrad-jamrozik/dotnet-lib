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
    int Missions,
    int Kills,
    int Rank,
    int MonthsService,
    float Recovery,
    int ManaMissing,
    int StatGainTotal,
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
    int CurrentMana)
{
    private static IEnumerable<PropertyInfo> Properties { get; } = typeof(Soldier).GetProperties();

    public static string CsvHeaders()
        => string.Join(",", 
            Properties.Select(p => p.Name)
                .Union(TrainingStats.CsvHeaders())
                .Union(MaxStats.CsvHeaders()));

    public override string ToString()
        => $"{nameof(Soldier)} {{ "
           + string.Join(", ", DataMap().Select(p => $"{p.Key} = {p.Value}")) + " }";

    public string CsvString()
    {
        var str = ToString();
        var csvStr = Regex.Replace(str, $"{nameof(Soldier)} {{(.*) }}", "$1");
        csvStr = Regex.Replace(csvStr, " \\w+\\ = ", "");
        return csvStr;
    }

    private IEnumerable<(string Key, object Value)> DataMap()
    {
        var propertyData = Properties.Select(p => (p.Name, p.GetValue(this)));
        var allData = propertyData
            .Union(TrainingStats.Get(this))
            .Union(MaxStats.Get(this));
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

    public string Helix => Name.EndsWith("H") ? "TRUE" : "FALSE";

    public string Humanoid => Type is "STR_SOLDIER" or "STR_HYBRID" ? "TRUE" : "FALSE";
}