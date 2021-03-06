using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Data
{
    public record TrieFromPaths<TPath, TSegment>
        (IEnumerable<TPath> Paths, Func<TPath, IEnumerable<TSegment>> ExtractSegments) 
        : Trie<object>(PathPrefixes(Paths, ExtractSegments)), IEnumerable<TPath> 
        where TSegment : IEquatable<TSegment>
    {
        private static IEnumerable<PathPart<object>> PathPrefixes(
            IEnumerable<TPath> paths,
            Func<TPath, IEnumerable<TSegment>> extractSegments)
        {
            IEnumerable<IEnumerable<TSegment>> pathsSegments = paths.Select(extractSegments);
            
            // kja new approach:
            // 1. Transpose the pathSegments, so first dimension is the depth, and the second dimension is the path.
            // --> (pathsSegments.Transpose())
            // 2. Then go from left (depth 0) to right (depth max). Continue as long as all the segments at given depth are the same.
            // Once they are no longer the same:
            // 1. create a prefix segment set,
            // 2. recursively build SegmentSets for all the suffixes
            // 3. prepend the prefix segment set to all the build suffix sets.

            return new List<PathPart<object>>();
        }

        public IEnumerator<TPath> GetEnumerator() => Paths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}