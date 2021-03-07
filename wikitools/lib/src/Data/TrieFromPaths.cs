using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data
{
    public record TrieFromPaths
        (IEnumerable<string> Paths, Func<string, IEnumerable<string>> ToSegments) 
        : Trie<object?>(RootPathPart(Paths, ToSegments)), IEnumerable<string> 
    {
        private static PathPart<object?> RootPathPart(
            IEnumerable<string> paths,
            Func<string, IEnumerable<string>> toSegments)
        {
            // Organize the paths by their segment at depth.
            // n-th entry is the collection of n-th segment from all paths.
            var pathsSegmentsByDepth = paths.Select(toSegments).Transpose();

            // Find the shared common prefix across all paths, i.e.
            // The first n segments that are the same for all paths.
            IEnumerable<IEnumerable<string>> pathsSegmentsByDepthSamePrefix = pathsSegmentsByDepth.TakeWhile(segmentsAtDepth => segmentsAtDepth.AllSame());

            // Select the first n segments that are the same for all paths.
            IEnumerable<string> samePrefix = pathsSegmentsByDepthSamePrefix.Select(segmentsAtDepth => segmentsAtDepth.First());

            var childPathParts = new List<PathPart<object?>>(); // kja build the suffixes recursively
            return new PathPart<object?>(samePrefix, null, childPathParts);
        }

        public IEnumerator<string> GetEnumerator() => Paths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}