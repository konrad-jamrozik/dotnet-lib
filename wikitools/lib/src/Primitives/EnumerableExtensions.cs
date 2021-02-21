using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Contracts;

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

            var firstKeySet      = Enumerable.ToHashSet(firstArray.Select(keySelector));
            var secondKeySet     = Enumerable.ToHashSet(secondArray.Select(keySelector));
            var intersectingKeys = Enumerable.ToHashSet(firstKeySet.Intersect(secondKeySet));

            var firstExceptSecond = firstKeySet.Except(secondKeySet).Select(key => firstByKey[key]);
            var secondExceptFirst = secondKeySet.Except(firstKeySet).Select(key => secondByKey[key]);
            var intersecting      = intersectingKeys.Select(key => intersector(firstByKey[key], secondByKey[key]));

            var union = firstExceptSecond.Union(intersecting).Union(secondExceptFirst).ToArray();

            Contract.Assert(union.DistinctBy(keySelector).Count() == union.Length, "Any given key appears only once");
            return union;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) => MoreEnumerable.DistinctBy(source, keySelector);

        public static void ForEach<TSource>(
            this IEnumerable<TSource> source,
            Action<TSource> action) => MoreEnumerable.ForEach(source, action);

        public static IExtremaEnumerable<TSource> MaxBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selector) => MoreEnumerable.MaxBy(source, selector);

        public static void AssertDistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var sourceArray = source as TSource[] ?? source.ToArray();
            if (MoreEnumerable.DistinctBy(sourceArray, keySelector).Count() != sourceArray.Length)
            {
                throw new InvariantException();
            }
        }

        public static void AssertOrderedBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var sourceArray = source as TSource[] ?? source.ToArray();
            var ordered     = sourceArray.OrderBy(keySelector).ToArray();
            if (!ordered.SequenceEqual(sourceArray))
            {
                throw new InvariantException();
            }
        }

        public static void Assert<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            try
            {
                MoreEnumerable.Assert(source, predicate);
            }
            catch (Exception e)
            {
                throw new InvariantException("MoreEnumerable.Assert failed", e);
            }
        }

        public static void AssertSingleBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            string message)
        {
            if (MoreEnumerable.DistinctBy(source, keySelector).Count() != 1)
            {
                throw new InvariantException(message);
            }
        }
    }
}