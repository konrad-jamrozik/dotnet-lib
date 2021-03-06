using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Data
{
    // kja wire this in, instead of TreeData
    public abstract record Trie<TValue>(IEnumerable<PathPrefix<TValue>> Prefixes)
    {
        public IEnumerable<(IList<string> Path, TValue value)> AsPreorderEnumerable() => PreorderTraversal();

        private IEnumerable<(IList<string> Path, TValue value)> PreorderTraversal()
        {
            IEnumerable<(IList<string> Path, TValue value)> preorderTree = PreorderTraversal(Prefixes);
            return preorderTree;
        }

        private static IEnumerable<(IList<string> Path, TValue value)> PreorderTraversal(IEnumerable<PathPrefix<TValue>> prefixes) =>
            prefixes.SelectMany(PreorderTraversal).ToList();

        private static IEnumerable<(IList<string> Path, TValue value)> PreorderTraversal(PathPrefix<TValue> prefix)
        {
            var currPrefix = new List<(IList<string> Path, TValue value)> { (prefix.PathSegments, prefix.Value) };
            var suffixes = prefix.PathSuffixes.Any()
                ? PreorderTraversal(prefix.PathSuffixes)
                : new List<(IList<string> Path, TValue value)>();
            return currPrefix.Concat(suffixes).ToList();
        }
    }
}