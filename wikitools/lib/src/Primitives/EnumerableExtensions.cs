using System;
using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Primitives
{
    public static class EnumerableExtensions
    {
        public static (TGroupKey key, TSource[] items)[] GroupAndOrderBy<TSource, TGroupKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TGroupKey> keySelector)
        {
            ILookup<TGroupKey, TSource>             lookup        = source.ToLookup(keySelector);
            IEnumerable<(TGroupKey Key, TSource[])> groups        = lookup.Select(g => (g.Key, g.ToArray()));
            (TGroupKey Key, TSource[])[]            orderedGroups = groups.OrderBy(t => t.Key).ToArray();
            return orderedGroups;
        }
    }
}