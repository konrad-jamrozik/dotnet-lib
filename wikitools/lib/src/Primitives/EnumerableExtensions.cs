using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static TSource[] UnionUsing<TSource, TKey>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TKey> keySelector,
            Func<TSource, TSource, TSource> intersector) where TKey : struct
        {
            var firstArray  = first as TSource[] ?? first.ToArray();
            var secondArray = second as TSource[] ?? second.ToArray();

            var firstByKey  = firstArray.ToDictionary(keySelector);
            var secondByKey = secondArray.ToDictionary(keySelector);

            var firstKeySet      = firstArray.Select(keySelector).ToHashSet();
            var secondKeySet     = secondArray.Select(keySelector).ToHashSet();
            var intersectingKeys = firstKeySet.Intersect(secondKeySet).ToHashSet();

            var firstExceptSecond = firstKeySet.Except(secondKeySet).Select(key => firstByKey[key]);
            var secondExceptFirst = secondKeySet.Except(firstKeySet).Select(key => secondByKey[key]);
            var intersecting      = intersectingKeys.Select(key => intersector(firstByKey[key], secondByKey[key]));

            var union = firstExceptSecond.Union(intersecting).Union(secondExceptFirst).ToArray();

            Debug.Assert(union.DistinctBy(keySelector).Count() == union.Length, "Any given key appears only once");
            return union;
        }
    }
}