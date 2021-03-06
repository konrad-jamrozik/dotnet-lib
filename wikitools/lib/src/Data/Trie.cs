using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data
{
    // kja wire this in, instead of TreeData
    public abstract record Trie<TValue>(IEnumerable<PathPrefix<TValue>> Prefixes)
    {
        public IEnumerable<PathPrefix<TValue>> PreorderTraversal() => PreorderTraversal(Prefixes);

        private static IEnumerable<PathPrefix<TValue>> PreorderTraversal(IEnumerable<PathPrefix<TValue>> prefixes) =>
            prefixes.SelectMany(PreorderTraversal);

        private static IEnumerable<PathPrefix<TValue>> PreorderTraversal(PathPrefix<TValue> prefix) =>
            prefix.WrapInList().Concat(
                prefix.PathSuffixes.Any()
                    ? PreorderTraversal(prefix.PathSuffixes)
                    : new List<PathPrefix<TValue>>());
    }

    // kja adapt. Now it will need also TValue, where for file system TValue is object which will be null (there is no 'void')
    public record TrieFromPaths<TPath, TSegment>
        (IEnumerable<TPath> Paths, Func<TPath, IEnumerable<TSegment>> ExtractSegments) 
        : Trie<object>(PathPrefixes(Paths, ExtractSegments)), IEnumerable<TPath> 
        where TSegment : IEquatable<TSegment>
    {
        private static IEnumerable<PathPrefix<object>> PathPrefixes(
            IEnumerable<TPath> paths,
            Func<TPath, IEnumerable<TSegment>> extractSegments)
        {
            IEnumerable<IEnumerable<TSegment>> pathsSegments = paths.Select(extractSegments);

            // kja new approach:
            // 1. Transpose the pathSegments, so first dimension is the depth, and the second dimension is the path.
            // 2. Then go from left (depth 0) to right (depth max). Continue as long as all the segments at given depth are the same.
            // Once they are no longer the same:
            // 1. create a prefix segment set,
            // 2. recursively build SegmentSets for all the suffixes
            // 3. prepend the prefix segment set to all the build suffix sets.


            // IEnumerable<PathPrefix<object>> nodes = PathPrefixesFromSegments(pathsSegments);
            return new List<PathPrefix<object>>();
        }

        public IEnumerator<TPath> GetEnumerator() => Paths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}