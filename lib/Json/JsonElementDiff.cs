using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using Wikitools.Lib.Contracts;
// For explanation of this alias, please see comment on Wikitools.Lib.Json.JsonElementDiff.Value.
using DiffObject = System.Object;

namespace Wikitools.Lib.Json;

public class JsonElementDiff
{
    private enum ComparisonResult
    {
        Equal,
        NotEqual,
        Recurse,
        AddedInTarget,
        MissingFromTarget,
    }

    private static readonly Dictionary<ComparisonResult, string> ComparisonResultSymbols = new()
    {
        [ComparisonResult.Equal] = "=",
        [ComparisonResult.NotEqual] = "!",
        [ComparisonResult.Recurse] = "*",
        [ComparisonResult.AddedInTarget] = "+",
        [ComparisonResult.MissingFromTarget] = "-"
    };

    private readonly Lazy<DiffObject?> _diff;

    public JsonElementDiff(JsonDocument baseline, JsonDocument target)
    {
        _diff = new Lazy<DiffObject?>(() => Diff(baseline, target));

        DiffObject? Diff(JsonDocument baseline, JsonDocument target)
        {
            var discardedPropName = string.Empty;
            (string name, DiffObject? value) diff = 
                ComputePropertyDiff(discardedPropName, baseline.RootElement, target.RootElement);

            return diff.value;
        }
    }

    /// <summary>
    /// An object representing the diff between the input baseline and target.
    /// This object can be serialized to json with JsonSerializer.Serialize, retaining all the diff information.
    ///
    /// If there are no differences between baseline and target then the value is null.
    /// </summary>
    /// <remarks>
    /// The type is DiffObject which, due to C# limitations, is modeled just as an 'object'.
    /// However, in fact, it is a recursive type that is a valid JsonElement. Here that JsonElement
    /// is used to represent, as json, the diff of given JsonElement between baseline and target,
    /// hence we alias it.
    ///
    /// It is recursive because the difference between two JsonElements might be in fact a set
    /// of differences in their children properties. The same logic applies recursively to these children.
    ///
    /// To actually obtain JsonElement from this Value, see Wikitools.Lib.Json.JsonDiff.JsonElement
    /// </remarks>
    public DiffObject? Value => _diff.Value;

    private static (string name, DiffObject? value) ComputePropertyDiff(string name, JsonElement baseline, JsonElement target)
    {
        (ComparisonResult res, string? msg) = CompareElements(baseline, target);

        return res switch
        {
            ComparisonResult.Equal => (name, null),
            ComparisonResult.NotEqual => Leaf(name, res, msg),
            ComparisonResult.Recurse => (name, value: ComputeElementsDiffRecursively(baseline, target)),
            _ => throw new ArgumentOutOfRangeException($"name: {name} res: {res}")
        };
    }

    private static (ComparisonResult res, string? msg) CompareElements(JsonElement baseline, JsonElement target)
    {
        const ComparisonResult eql = ComparisonResult.Equal;
        const ComparisonResult neq = ComparisonResult.NotEqual;
        const ComparisonResult rec = ComparisonResult.Recurse;

        var baselineValueKind = baseline.ValueKind;
        var targetValueKind = target.ValueKind;
        if (baselineValueKind != targetValueKind)
            return (neq, "ValueKind " +
                         $"baseline: {baselineValueKind} ({baseline}) | " +
                         $"target: {targetValueKind} ({target})");

        return baselineValueKind switch
        {
            JsonValueKind.Number when baseline.GetDecimal() != target.GetDecimal() 
                => (neq, $"baseline: {baseline} | target: {target}"),
            JsonValueKind.String when baseline.GetString() != target.GetString() 
                => (neq, $"baseline: {baseline} | target: {target}"),

            JsonValueKind.Number    => (eql, null),
            JsonValueKind.String    => (eql, null),
            JsonValueKind.Undefined => (eql, null),
            JsonValueKind.Null      => (eql, null),
            JsonValueKind.False     => (eql, null),
            JsonValueKind.True      => (eql, null),

            JsonValueKind.Object    => (rec, null),
            JsonValueKind.Array     => (rec, null),

            _ => throw new ArgumentOutOfRangeException(baselineValueKind.ToString())
        };
    }

    private static IDictionary<string, DiffObject>? ComputeElementsDiffRecursively(JsonElement baseline, JsonElement target)
    {
        var baselineValueKind = baseline.ValueKind;

        Contract.Assert(baselineValueKind == target.ValueKind);
        Contract.Assert(baselineValueKind == JsonValueKind.Object 
                        || baselineValueKind == JsonValueKind.Array);

        IDictionary<string, DiffObject> result = baselineValueKind == JsonValueKind.Object
            ? ComputeObjectsDiff(baseline, target)
            : ComputeArraysDiff(baseline, target);

        return result.Any() ? result : null;
    }

    private static IDictionary<string, DiffObject> ComputeObjectsDiff(JsonElement baseline, JsonElement target)
    {
        var baselineProps = baseline.EnumerateObject().ToDictionary(prop => prop.Name);
        var targetProps = target.EnumerateObject().ToDictionary(prop => prop.Name);

        var propsShared = baselineProps.Keys.Intersect(targetProps.Keys);
        var propsAddedInTarget = targetProps.Keys.Except(baselineProps.Keys);
        var propsMissingInTarget = baselineProps.Keys.Except(targetProps.Keys);

        var intersecting = propsShared
            .Select(propName => ComputePropertyDiff(propName, baselineProps[propName].Value, targetProps[propName].Value))
            .Where(diff => diff.value is not null)
            // ReSharper disable once RedundantEnumerableCastCall
            // - Actually needed to satisfy nullability analysis
            .Cast<(string name, object value)>();

        var added = propsAddedInTarget
            .Select(propName => Leaf(propName, ComparisonResult.AddedInTarget, msg: null));
            
        var missing = propsMissingInTarget
            .Select(propName => Leaf(propName, ComparisonResult.MissingFromTarget, msg: null));

        var result = intersecting.Concat(added).Concat(missing).ToDictionary(t => t.name, t => t.value);
        return result;
    }

    private static IDictionary<string, DiffObject> ComputeArraysDiff(JsonElement baseline, JsonElement target)
    {
        var baselineElems = baseline.EnumerateArray().ToImmutableArray();
        var targetElems = target.EnumerateArray().ToImmutableArray();

        var intersecting = Enumerable.Range(0, Math.Min(baselineElems.Length, targetElems.Length))
            .Select(i => ComputePropertyDiff(name: i.ToString(), baselineElems[i], targetElems[i]))
            .Where(diff => diff.value is not null)
            // ReSharper disable once RedundantEnumerableCastCall
            // - Actually needed to satisfy nullability analysis
            .Cast<(string name, object value)>();

        var added = Enumerable.Range(baselineElems.Length, Math.Max(targetElems.Length - baselineElems.Length, 0))
            .Select(i => Leaf(name: i.ToString(), ComparisonResult.AddedInTarget, msg: null));

        var missing = Enumerable.Range(targetElems.Length, Math.Max(baselineElems.Length - targetElems.Length, 0))
            .Select(i => Leaf(name: i.ToString(), ComparisonResult.MissingFromTarget, msg: null));

        var result = intersecting.Concat(added).Concat(missing).ToDictionary(t => t.name, t => t.value);
        return result;
    }

    private static (string name, DiffObject value) Leaf(string name, ComparisonResult res, string? msg) 
        => (name, $"{ComparisonResultSymbols[res]} {msg}".Trim());
}