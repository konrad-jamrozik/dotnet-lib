using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data;

public abstract record Trie<TValue>(PathPart<TValue> RootPathPart)
{
    public IEnumerable<PathPart<TValue>> PreorderTraversal(bool leafsOnly = true)
    {
        var rootPreorderTraversal = RootPreorderTraversal;
        return leafsOnly 
            ? rootPreorderTraversal.Where(path => !path.Suffixes.Any()) 
            : rootPreorderTraversal;
    }

    private IEnumerable<PathPart<TValue>> RootPreorderTraversal => RootPathPart switch
    {
        // The .segments.Any() filter here ensures that if the input enumerable is empty,
        // the returned enumerable is empty instead of being a collection with one PathPart which is empty.
        var (segments, _, _) when segments.Any() => PreorderTraversal(RootPathPart),
        // This case ensures that if the input RootPathPart has empty prefix, the preorder still
        // is done instead of the method returning empty value.
        var (_, _, suffixes) when suffixes.Any() => PreorderTraversal(RootPathPart.Suffixes),
        _ => Array.Empty<PathPart<TValue>>()
    };

    private static IEnumerable<PathPart<TValue>> PreorderTraversal(PathPart<TValue> prefixPathPart)
    {
        var traversedSuffixes = PreorderTraversal(prefixPathPart.Suffixes);
        // The .Concat here ensures that each pathPart starts with the prefixPathPart.
        var pathParts = traversedSuffixes.Select(prefixPathPart.Concat);
        // The .Concat here ensure the prefixPathPart itself is prepended to the list
        // of returned path parts.
        return prefixPathPart.WrapInList().Concat(pathParts);
    }

    private static IEnumerable<PathPart<TValue>> PreorderTraversal(IEnumerable<PathPart<TValue>> pathParts)
        => pathParts.SelectMany(PreorderTraversal).ToList();
}