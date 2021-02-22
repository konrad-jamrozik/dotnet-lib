using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives
{
    public static class EnumerableExtensions
    {
        public static (TGroupKey key, TSource[] items)[] GroupAndOrderBy<TSource, TGroupKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TGroupKey> selectKey)
        {
            ILookup<TGroupKey, TSource>             lookup        = source.ToLookup(selectKey);
            IEnumerable<(TGroupKey Key, TSource[])> groups        = lookup.Select(g => (g.Key, g.ToArray()));
            (TGroupKey Key, TSource[])[]            orderedGroups = groups.OrderBy(t => t.Key).ToArray();
            return orderedGroups;
        }

        public static TSource[] UnionUsing<TSource, TKey>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TKey> selectKey,
            Func<TSource, TSource, TSource> intersect) where TKey : struct
        {
            var firstArray  = first as TSource[] ?? first.ToArray();
            var secondArray = second as TSource[] ?? second.ToArray();

            var firstByKey  = firstArray.ToDictionary(selectKey);
            var secondByKey = secondArray.ToDictionary(selectKey);

            var firstKeySet      = firstArray.Select(selectKey).ToHashSet();
            var secondKeySet     = secondArray.Select(selectKey).ToHashSet();
            var intersectingKeys = firstKeySet.Intersect(secondKeySet).ToHashSet();

            var firstExceptSecond =
                firstKeySet.Except(secondKeySet).Select(key => firstByKey[key]); // kja test for this .Except
            var secondExceptFirst = secondKeySet.Except(firstKeySet).Select(key => secondByKey[key]);
            var intersecting      = intersectingKeys.Select(key => intersect(firstByKey[key], secondByKey[key]));

            var union = firstExceptSecond.Union(intersecting).Union(secondExceptFirst).ToArray();

            union.AssertDistinctBy(selectKey);
            return union;
        }
    }
}