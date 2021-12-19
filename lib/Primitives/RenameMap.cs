using System;
using System.Collections.Generic;
using System.Linq;

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

    private static IDictionary<string, string> FinalNamesMap(IEnumerable<(string from, string to)> renames)
    {
        // kja FinalNamesMap
        // 1. Function that takes as input a list of pairs: (string from, string to).
        //    Returns: a hashmap: {key: string from, value: string finalTo }
        throw new NotImplementedException();
    }

    public ILookup<string, T> Apply<T>(ILookup<string, T> collectionMap)
    {
        // kja Apply
        // 2. Function that takes as input a hashmap {key: string, value: enumerable }
        //    Returns: hashmap, where all the enumerables having the same finalTo
        //    are merged into one, under the "finalTo" key.
        throw new NotImplementedException();
    }

}