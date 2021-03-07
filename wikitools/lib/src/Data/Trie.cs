using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data
{
    // kja wire this in, instead of TreeData
    public abstract record Trie<TValue>(PathPart<TValue> RootPathPart)
    {
        public IEnumerable<PathPart<TValue>> PreorderTraversal() => PreorderTraversal(RootPathPart);

        /// <summary>
        /// This traversal implementation will behave as follows:
        /// (a) Each returned path part will have all segments of the part itself
        /// (b) Each returned path part will also have all segments of prefix path parts
        /// prepended.
        /// (c) There is entry in the result for each path part that has no suffixes.
        /// (d) There is entry in the result for each path part that has at least one suffix.
        /// </summary>
        private static IEnumerable<PathPart<TValue>> PreorderTraversal(PathPart<TValue> pathPart)
        {
            // This variable contains the segments providing property (a)
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