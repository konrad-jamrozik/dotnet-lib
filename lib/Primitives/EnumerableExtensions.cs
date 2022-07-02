using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives;

public static class EnumerableExtensions
{
    /// <summary>
    /// Returns source filtered to these elements, whose property, of type
    /// string, doesn't contain any of the excludedSubstrings.
    /// </summary>
    public static IEnumerable<T> WhereNotContains<T>(
        this IEnumerable<T> source,
        Func<T, string> property,
        IEnumerable<string>? excludedSubstrings)
    {
        var excludedSubstringsArr = excludedSubstrings as string[] ?? excludedSubstrings?.ToArray();
        return excludedSubstringsArr == null || !excludedSubstringsArr.Any()
            ? source
            : source.Where(
                item => !excludedSubstringsArr.Any(
                    excludedSubstring => property(item).Contains(excludedSubstring)));
    }

    public static (TGroupKey key, TSource[] items)[] GroupAndOrderBy<TSource, TGroupKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TGroupKey> selectKey)
    {
        ILookup<TGroupKey, TSource>             lookup        = source.ToLookup(selectKey);
        IEnumerable<(TGroupKey Key, TSource[])> groups        = lookup.Select(g => (g.Key, g.ToArray()));
        (TGroupKey Key, TSource[])[]            orderedGroups = groups.OrderBy(t => t.Key).ToArray();
        return orderedGroups;
    }

    public static TSource[] ConcatMerge<TSource, TKey>(
        this IEnumerable<TSource> first,
        IEnumerable<TSource> second,
        Func<TSource, TKey> selectKey,
        Func<TSource, TSource, TSource> merge) where TKey : notnull
    {
        var firstArray  = first as TSource[] ?? first.ToArray();
        var secondArray = second as TSource[] ?? second.ToArray();

        var firstByKey  = firstArray.ToDictionary(selectKey);
        var secondByKey = secondArray.ToDictionary(selectKey);

        var firstKeySet     = Enumerable.ToHashSet(firstArray.Select(selectKey));
        var secondKeySet    = Enumerable.ToHashSet(secondArray.Select(selectKey));
        var overlappingKeys = Enumerable.ToHashSet(firstKeySet.Intersect(secondKeySet));

        var firstExceptSecond = firstKeySet.Except(secondKeySet).Select(key => firstByKey[key]);
        var secondExceptFirst = secondKeySet.Except(firstKeySet).Select(key => secondByKey[key]);
        var overlapping       = overlappingKeys.Select(key => merge(firstByKey[key], secondByKey[key]));

        var concat = firstExceptSecond.Concat(overlapping).Concat(secondExceptFirst).ToArray();

        concat.AssertDistinctBy(selectKey);
        return concat;
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
                    $"Items do not match. " +
                    $"currFirstItem: '{currFirstItem}' " +
                    $"currSecondItem: '{currSecondItem}'");

            yield return selectResult(currFirstItem, currSecondItem);
        }

        if (secondEnumerator.MoveNext())
            throw new InvalidOperationException();
    }

    public static (
        IEnumerable<(TLeft, TRight)> Both,
        IEnumerable<TLeft> Left,
        IEnumerable<TRight> Right
        )
        FullJoinToSegments<TLeft, TRight, TKey>(
            this IEnumerable<TLeft> first,
            IEnumerable<TRight> second,
            Func<TLeft, TKey> firstKeySelector,
            Func<TRight, TKey> secondKeySelector)
        where TLeft : class
        where TRight : class
    {
        List<(TLeft? left, TRight? right)> fullJoin = first.FullJoin(second,
                firstKeySelector,
                secondKeySelector,
                firstSelector: firstElem => (firstElem, null),
                secondSelector: secondElem => (null, secondElem),
                bothSelector: (firstElem, secondElem) => ((TLeft?) firstElem, (TRight?) secondElem))
            .ToList();

        IEnumerable<(TLeft, TRight)> both = 
            fullJoin.Where(e => e is { left: { }, right: { } })!;
            
        IEnumerable<TLeft> left = fullJoin
            .Where(e => e is { left: { }, right: null })
            .Select(e => e.left)!;
            
        IEnumerable<TRight> right = fullJoin
            .Where(e => e is { left: null, right: { } })
            .Select(e => e.right)!;

        return (both, left, right);
    }
}