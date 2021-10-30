using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data
{
    public abstract record Trie<TValue>(PathPart<TValue> RootPathPart)
    {
        public IEnumerable<PathPart<TValue>> PreorderTraversal() => RootPathPart switch
        {
            var (segments, _, _) when segments.Any() => PreorderTraversal(RootPathPart),
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
            return prefixPathPart.AsList().Concat(pathParts);
        }

        private static IEnumerable<PathPart<TValue>> PreorderTraversal(IEnumerable<PathPart<TValue>> pathParts) 
            => pathParts.SelectMany(PreorderTraversal);
    }
}