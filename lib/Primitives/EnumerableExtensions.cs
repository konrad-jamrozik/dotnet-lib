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

        public static TSource[] UnionMerge<TSource, TKey>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TKey> selectKey,
            Func<TSource, TSource, TSource> merge) where TKey : notnull
        {
            var firstArray  = first as TSource[] ?? first.ToArray();
            var secondArray = second as TSource[] ?? second.ToArray();

            var firstByKey  = firstArray.ToDictionary(selectKey);
            var secondByKey = secondArray.ToDictionary(selectKey);

            var firstKeySet     = firstArray.Select(selectKey).ToHashSet();
            var secondKeySet    = secondArray.Select(selectKey).ToHashSet();
            var overlappingKeys = firstKeySet.Intersect(secondKeySet).ToHashSet();

            var firstExceptSecond = firstKeySet.Except(secondKeySet).Select(key => firstByKey[key]);
            var secondExceptFirst = secondKeySet.Except(firstKeySet).Select(key => secondByKey[key]);
            var overlapping       = overlappingKeys.Select(key => merge(firstByKey[key], secondByKey[key]));

            var union = firstExceptSecond.Union(overlapping).Union(secondExceptFirst).ToArray();

            union.AssertDistinctBy(selectKey);
            return union;
        }

        public static void AssertOrderedBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selectKey)
        {
            var sourceArray = source as TSource[] ?? source.ToArray();
            var ordered     = sourceArray.OrderBy(selectKey).ToArray();
            if (!ordered.SequenceEqual(sourceArray))
            {
                throw new InvariantException();
            }
        }

        public static bool AllSame<TSource>(
            this IEnumerable<TSource> source)
            => source.Distinct().Count() == 1;

        public static void AssertSameBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selectKey,
            string message)
        {
            if (!source.AllSameBy(selectKey))
            {
                throw new InvariantException(message);
            }
        }

        public static IEnumerable<TResult> ZipMatching<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, bool> match,
            Func<TFirst, TSecond, TResult> selectResult)
        {
            using var firstEnumerator = first.GetEnumerator();
            using var secondEnumerator = second.GetEnumerator();

            while (firstEnumerator.MoveNext())
            {
                var currSecondItemFound = secondEnumerator.MoveNext();

                if (!currSecondItemFound)
                    throw new InvalidOperationException();

                var currFirstItem = firstEnumerator.Current;
                var currSecondItem = secondEnumerator.Current;

                if (!match(currFirstItem, currSecondItem))
                    throw new InvalidOperationException(
                        $"Items do not match. currFirstItem: '{currFirstItem}' currSecondItem: '{currSecondItem}'");

                yield return selectResult(currFirstItem, currSecondItem);
            }

            if (secondEnumerator.MoveNext())
                throw new InvalidOperationException();
        }
    }
}