﻿using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives
{
    public static class EnumerableMoreLinqExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selectKey) => MoreEnumerable.DistinctBy(source, selectKey);

        public static void ForEach<TSource>(
            this IEnumerable<TSource> source,
            Action<TSource> action) => MoreEnumerable.ForEach(source, action);

        public static IExtremaEnumerable<TSource> MaxBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selectKey) => MoreEnumerable.MaxBy(source, selectKey);

        public static void AssertDistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selectKey)
        {
            var sourceArray = source as TSource[] ?? source.ToArray();
            if (MoreEnumerable.DistinctBy(sourceArray, selectKey).Count() != sourceArray.Length)
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
                throw new InvariantException("MoreEnumerable.Assert threw", e);
            }
        }


        public static bool AllSameBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selectKey)
            => MoreEnumerable.DistinctBy(source, selectKey).Count() == 1;
    }
}