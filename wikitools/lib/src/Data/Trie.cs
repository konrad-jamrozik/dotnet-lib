using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data
{
    // kja wire this in, instead of TreeData
    public abstract record Trie<TValue>(IEnumerable<PathPrefix<TValue>> Prefixes)
    {
        public IEnumerable<PathPrefix<TValue>> PreorderTraversal() => PreorderTraversal(Prefixes);

        private static IEnumerable<PathPrefix<TValue>> PreorderTraversal(IEnumerable<PathPrefix<TValue>> prefixes) =>
            prefixes.SelectMany(PreorderTraversal);

        private static IEnumerable<PathPrefix<TValue>> PreorderTraversal(PathPrefix<TValue> prefix) =>
            prefix.WrapInList().Concat(
                prefix.PathSuffixes.Any()
                    ? PreorderTraversal(prefix.PathSuffixes)
                    : new List<PathPrefix<TValue>>());
    }
}