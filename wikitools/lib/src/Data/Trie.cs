using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data
{
    // kja wire this in, instead of TreeData
    public abstract record Trie<TValue>(PathPart<TValue> RootPathPart)
    {
        public IEnumerable<PathPart<TValue>> PreorderTraversal() => PreorderTraversal(RootPathPart);

        private static IEnumerable<PathPart<TValue>> PreorderTraversal(PathPart<TValue> pathPart) =>
            pathPart.WrapInList().Concat(
                // kja I think that here the pathPart needs to be prepended to all the children.
                // Otherwise only the last PathPart will be returned.
                // So something like PathPart.Prepend(IEnumerable)
                //
                // kja in general, options like that:
                // - return PathParts that have nonempty Suffixes, or not
                // - split each PathParts into a sequence of PathParts, each with one segment, or not.
                // - for each returned PathPart, include all the segments from all the preceding path parts, or not.
                // For wiki I can say the following:
                // - I need each PathPart with nonempty suffix, as it might be a page or dir with subpages
                // - I need path each segment independently, as segment is a page or dir
                //   - However, this should be reflected by the input paths to wiki, so don't really need it.
                // - I need all the preceding segments, so I can construct full URL to the page.

                pathPart.Suffixes.Any()
                    ? PreorderTraversal(pathPart.Suffixes).Select(pathPart.Concat)
                    : new List<PathPart<TValue>>());

        private static IEnumerable<PathPart<TValue>> PreorderTraversal(IEnumerable<PathPart<TValue>> prefixes) =>
            prefixes.SelectMany(PreorderTraversal);
    }
}