using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives;

// Find all renames
// If given file path is a 'before' name in any of the renames,
// then add it to the 'after' group
//
// Consider case:
// a -> b
// b -> c
//
// Here the algorithm will first add a to b. Then it would find rename
// from b to c, adding b to c, resulting in abc in one group.
//
// Optimization idea:
//
// Come up with chains like "abc ends up being d", "gh ends up being i".
// Then go another groupBy on the existing groupBy, where they would be
// grouped on the new property "finalName".
//
// So, algorithm 2:
//
// 1. Go through all the renames, build the chain of renames.
// 2. For each existing group, add the new property of final name;
// 3. Then group by that new property.
//
// Generalization of algorithm 2:
//
// 1. Function that takes as input a list of pairs: (string from, string to).
//    Returns: a hashmap: {key: string from, value: string finalTo }
// 2. Function that takes as input a hashmap {key: string, value: enumerable }
//    Returns: hashmap, where all the enumerables having the same finalTo
//    are merged into one, under the "finalTo" key.
public record RenameMap(IEnumerable<(string from, string to)> Renames)
{
    private readonly IDictionary<string, string> _finalNamesMap = FinalNamesMap(Renames);

    public ILookup<string, T> Apply<T>(ILookup<string, T> lookup)
    {
        // kja Apply
        // 2. Function that takes as input a hashmap {key: string, value: enumerable }
        //    Returns: hashmap, where all the enumerables having the same finalTo
        //    are merged into one, under the "finalTo" key.
        throw new NotImplementedException();
    }

    private static IDictionary<string, string> FinalNamesMap(
        IEnumerable<(string from, string to)> renames)
    {
        var toFromMap = ToFromMap(renames);
        var fromToMap = FromToMap(toFromMap);

        return fromToMap;
    }

    private static Dictionary<string, string> FromToMap(Dictionary<string, List<string>> toFromMap)
    {
        var fromToMap = new Dictionary<string, string>(
            toFromMap.SelectMany(
                toFrom =>
                {
                    var (to, fromNames) = toFrom;
                    return fromNames.Select(from => new KeyValuePair<string, string>(from, to));
                }));
        return fromToMap;
    }

    
    /// <summary>
    /// If the input renames where:
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
                    var existingChain = toFromMap[to];
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
}