using System;
using System.Collections.Generic;
using MoreLinq;

namespace Wikitools.Lib.Primitives
{
    public static class EnumerableMoreLinqExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) => MoreEnumerable.DistinctBy(source, keySelector);

        public static void ForEach<TSource>(
            this IEnumerable<TSource> source,
            Action<TSource> action) => MoreEnumerable.ForEach(source, action);
    }
}