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

        /// <summary>
        /// This traversal implementation will behave as follows:
        /// (a) each returned path part will have all segments of the part itself
        /// and (b) each returned path part will also have all segments of prefix path parts prepended.
        /// and (c) there will be entry in the result for each path part that has no suffixes.
        /// and (d) there will be entry in the result for each path part that has at least one suffix.
        /// </summary>
        private static IEnumerable<PathPart<TValue>> PreorderTraversal(PathPart<TValue> pathPart)
        {
            // This variable contains the segments providing property (a)
            // Note that currentPathPart.Suffixes is nonempty, but redundant, as all suffixes will be
            // returned as other elements of the return enumerable, thanks to property (d).
            var currentPathPart = pathPart.WrapInList();
            // The .Select(pathPart.Concat) provides property (b)
            var suffixes = PreorderTraversal(pathPart.Suffixes).Select(pathPart.Concat);
            // Excluding currentPathPart if there are no suffixes would negate property (c)
            // Excluding currentPathPart if there are    suffixes would negate property (d)
            return currentPathPart.Concat(suffixes);
        }

        private static IEnumerable<PathPart<TValue>> PreorderTraversal(IEnumerable<PathPart<TValue>> pathParts) 
            => pathParts.SelectMany(PreorderTraversal);
    }
}