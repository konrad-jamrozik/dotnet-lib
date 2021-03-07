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

            return BuildTrie(segmentsByDepth);
        }

        private static PathPart<object?> BuildTrie(IList<IEnumerable<string>> segmentsByDepth)
        {
            // Find the shared common prefix across all paths, i.e.
            // The first n segments that are the same for all paths.
            var segmentsSamePrefix = segmentsByDepth.TakeWhile(segmentsAtDepth => segmentsAtDepth.AllSame());

            // Select the first n segments that are the same for all paths.
            var samePrefix = segmentsSamePrefix.Select(segmentsAtDepth => segmentsAtDepth.First()).ToList();

            var segmentsSuffixes = segmentsByDepth.Skip(samePrefix.Count).ToList();

            var suffixPathsParts = BuildSuffixes(segmentsSuffixes);
            return new PathPart<object?>(samePrefix, null, suffixPathsParts);
        }

        private static List<PathPart<object?>> BuildSuffixes(IList<IEnumerable<string>> segmentsByDepth)
        {
            if (!segmentsByDepth.Any())
                return new List<PathPart<object?>>();

            // kja is it possible to avoid duplicating this call, by restructuring the recursion?
            // Or maybe pass around both segmentsByDepth and segmentsByPath
            var paths = segmentsByDepth.Transpose().ToList();

            var prefixes = segmentsByDepth.First().Distinct();
            var pathsWithPrefix = prefixes.Select(prefix => paths.Where(pathSegments => pathSegments.First() == prefix));
            var suffixes = pathsWithPrefix.Select(pathWithPrefix => BuildTrie(pathWithPrefix.Transpose().ToList()));
            return suffixes.ToList();
        }

        public IEnumerator<string> GetEnumerator() => Paths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}