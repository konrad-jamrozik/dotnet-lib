using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives;

public record RenameMap(IEnumerable<(string from, string to)> Renames)
{
    private readonly IDictionary<string, string> _finalNamesMap = FinalNamesMap(Renames);

    public ILookup<string, T> Apply<T>(ILookup<string, T> lookup)
    {
        ILookup<string, T> applied = lookup.SelectMany(
            group =>
            {
                var groupName = group.Key;
                var renamedGroup = _finalNamesMap.ContainsKey(groupName)
                    ? group.Select(e => (_finalNamesMap[groupName], e))
                    : group.Select(e => (groupName, e));
                return renamedGroup;
            }).ToLookup();
        return applied;
    }

    private static IDictionary<string, string> FinalNamesMap(
        IEnumerable<(string from, string to)> renames)
    {
        var toFromMap = ToFromMap(renames);
        var fromToMap = FromToMap(toFromMap);
        return fromToMap;
    }

    /// <summary>
    /// If the input renames are:
    /// a -> b
    /// b -> c
    /// c -> d
    /// Then the toFromMap will contains "rename chain" like this:
    /// d -> [c, b, a]
    /// </summary>
    private static Dictionary<string, List<string>> ToFromMap(
        IEnumerable<(string from, string to)> renames)
    {
        // Assert: renames are in order of happening
        // An example INVALID input:
        // d -> e
        // b -> c
        // c -> d // INVALID rename. d already doesn't exist; it is "e" already.

        // renamedValues is used for correctness checking.
        // It is not returned.
        var renamedValues = new HashSet<string>();

        var toFromMap = renames.Aggregate(
            new Dictionary<string, List<string>>(),
            (toFromMap, rename) =>
            {
                var (from, to) = rename;

                if (renamedValues.Contains(from))
                    throw new InvariantException(
                        $"Cannot rename '{from}' as it was already renamed. " +
                        "This invariant violation possibly denotes violation of following " +
                        "precondition: 'renames have to be provided in chronological order'.");
                
                renamedValues.Add(from);

                // IF (there exists a 'rename chain' whose final 'to' ("existingTo")
                // is the same as current 'from')
                // THEN (extend that chain, so that the new final 'to'
                // is current 'to')
                //
                // Example:
                // existingChain:
                //   d <- [c, b, a]
                //   - here the final 'to' (existingTo) is "d"
                // Current (from, to):
                //   d -> e
                //   - here the current 'from' is "d"
                // newChain:
                //   e <- [d, c, b, a]
                if (toFromMap.ContainsKey(from))
                {
                    var existingTo = from;
                    var existingChain = toFromMap[from];
                    var newChain = new List<string>(existingChain);
                    newChain.Insert(0, from);
                    toFromMap.Add(to, newChain);
                    toFromMap.Remove(existingTo);

                    // We need to remove 'to' as it might have been renamed in the past.
                    // Consider this example:
                    // a -> b // "a" is considered renamed after this.
                    // b -> a // "a", which is "to", is no longer renamed and exists again.
                    if (!renamedValues.Remove(to))
                    {
                        // Do nothing. This just means that the "to" was never
                        // renamed before.
                    }
                }
                else
                    toFromMap[to] = new List<string> { from };

                return toFromMap;
            });
        return toFromMap;
    }

    private static Dictionary<string, string> FromToMap(Dictionary<string, List<string>> toFromMap)
    {
        var fromToMap = new Dictionary<string, string>(
            toFromMap.SelectMany(
                toFrom =>
                {
                    var (finalTo, fromNames) = toFrom;
                    
                    // Creating a HashSet is done here to ensure distinctness.
                    // This is necessary to handle rename loops.
                    //
                    // Consider an example of renames with a rename loop:
                    //
                    // a -> b
                    // b -> c
                    // c -> a // here we close the rename loop.
                    // a -> d
                    //
                    // Here the finalTo would be "d" and fromNames would be [a, c, b, a].
                    // Observe "a" appears twice in "fromNames".
                    // This would result in ArgumentException when constructing Dictionary.
                    // fromNamesSet instead is {a, b, c}.
                    //
                    // We lose the order of renames, but currently this is not needed for anything.
                    var fromNamesSet = new HashSet<string>(fromNames);

                    return fromNamesSet.Select(
                        from => new KeyValuePair<string, string>(from, finalTo));
                }));
        return fromToMap;
    }
}