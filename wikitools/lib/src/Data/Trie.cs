using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data
{
    // kja wire this in, instead of TreeData
    public abstract record Trie<TValue>(IEnumerable<PathPart<TValue>> Prefixes)
    {
        public IEnumerable<PathPart<TValue>> PreorderTraversal() => PreorderTraversal(Prefixes);

        private static IEnumerable<PathPart<TValue>> PreorderTraversal(IEnumerable<PathPart<TValue>> prefixes) =>
            prefixes.SelectMany(PreorderTraversal);

        private static IEnumerable<PathPart<TValue>> PreorderTraversal(PathPart<TValue> part) =>
            part.WrapInList().Concat(
                // kja I think that here the part needs to be prepended to all the children.
                // Otherwise only the last PathPart will be returned.
                // So something like PathPart.Prepend(IEnumerable
                part.Children.Any()
                    ? PreorderTraversal(part.Children)
                    : new List<PathPart<TValue>>());
    }

    // kja adapt. Now it will need also TValue, where for file system TValue is object which will be null (there is no 'void')
}