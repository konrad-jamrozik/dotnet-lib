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
            var segmentsByDepth = paths.Select(toSegments).Transpose().ToList();

            return BuildPathPart(segmentsByDepth);
        }

        private static PathPart<object?> BuildPathPart(IList<IEnumerable<string>> segmentsByDepth)
        {
            // Find the shared common prefix across all paths, i.e.
            // The first n segments that are the same for all paths.
            var segmentsSamePrefix = segmentsByDepth.TakeWhile(segmentsAtDepth => segmentsAtDepth.AllSame());

            // Select the first n segments that are the same for all paths.
            var samePrefix = segmentsSamePrefix.Select(segmentsAtDepth => segmentsAtDepth.First()).ToList();

            var segmentsSuffixes = segmentsByDepth.Skip(samePrefix.Count);

            var suffixPathsParts = BuildSuffixes(segmentsSuffixes);
            return new PathPart<object?>(samePrefix, null, suffixPathsParts);
        }

        private static List<PathPart<object?>> BuildSuffixes(IEnumerable<IEnumerable<string>> segmentsSuffixes)
        {
            if (!segmentsSuffixes.Any())
                return new List<PathPart<object?>>();

            return new List<PathPart<object?>>();
        }

        public IEnumerator<string> GetEnumerator() => Paths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}