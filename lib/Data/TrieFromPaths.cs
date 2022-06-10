using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data;

public record TrieFromPaths
    (IEnumerable<string> Paths, Func<string, IEnumerable<string>> ToSegments) 
    : Trie<object?>(Trie(Paths, ToSegments)), IEnumerable<string> 
{
    private static PathPart<object?> Trie(
        IEnumerable<string> paths,
        Func<string, IEnumerable<string>> toSegments)
    {
        var pathsSegments = paths.Select(toSegments).ToList();
        return PathPart(pathsSegments);
    }

    private static PathPart<object?> PathPart(IList<IEnumerable<string>> pathsSegments)
    {
        // Organize the paths by their segment at depth.
        // n-th entry is the collection of n-th segment for each path.
        var segmentsByDepth = pathsSegments.Transpose().ToList();

        var prefix = SamePrefix(segmentsByDepth);

        var pathsSuffixes = pathsSegments.Select(pathSegments => pathSegments.Skip(prefix.Count));

        var segmentsByDepthSuffixes = segmentsByDepth.Skip(prefix.Count).ToList();
            
        var suffixes = Suffixes(pathsSuffixes, segmentsByDepthSuffixes);

        // kj2-toc PathPart.Value / Currently value is null here as weaving values in
        // when building a trie from a path is not yet supported.
        return new PathPart<object?>(prefix, Value: null, suffixes);
    }

    private static IEnumerable<PathPart<object?>> Suffixes(
        IEnumerable<IEnumerable<string>> pathsSegments,
        IList<IEnumerable<string>> segmentsByDepth)
    {
        if (!segmentsByDepth.Any())
            return new List<PathPart<object?>>();

        var firstSegmentsOfSuffixes = segmentsByDepth.First().Distinct();
        var pathsWithPrefix = firstSegmentsOfSuffixes
            .Select(prefix => pathsSegments.Where(segments => segments.FirstOrDefault() == prefix).ToList());

        var suffixes = pathsWithPrefix.Select(PathPart);
        return suffixes.ToList();
    }

    /// <summary>
    /// Select the first n segments that are the same for all paths.
    /// The paths are represented by segmentsByDepth.
    /// </summary> 
    private static IList<string> SamePrefix(IList<IEnumerable<string>> segmentsByDepth)
    {
        // Find the shared common prefix across all paths, i.e.
        // the first n segments that are the same for all paths.
        var segmentsSamePrefix = segmentsByDepth.TakeWhile(segmentsAtDepth => segmentsAtDepth.AllSame());

        // Select the first n segments that are the same for all paths.
        // Note that .First() is arbitrary, as the prefix is common for all paths, not just the first.
        var samePrefix = segmentsSamePrefix.Select(segmentsAtDepth => segmentsAtDepth.First()).ToList();
        return samePrefix;
    }

    public IEnumerator<string> GetEnumerator() => Paths.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}