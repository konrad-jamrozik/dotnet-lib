using System;
using System.Collections;
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

    // kja adapt. Now it will need also TValue, where for file system TValue is object which will be null (there is no 'void')
    public record TrieFromPaths<TPath, TSegment>
        (IEnumerable<TPath> Paths, Func<TPath, IEnumerable<TSegment>> ExtractSegments) 
        : TreeData<TSegment>(TreeNodes(Paths, ExtractSegments)), IEnumerable<TPath> 
        where TSegment : IEquatable<TSegment>
    {
        private static IEnumerable<TreeNode<TSegment>> TreeNodes(
            IEnumerable<TPath> paths,
            Func<TPath, IEnumerable<TSegment>> extractSegments)
        {
            IEnumerable<IEnumerable<TSegment>> pathsSegments = paths.Select(extractSegments);
            IEnumerable<TreeNode<TSegment>> nodes = TreeNodesFromSegments(pathsSegments);
            return nodes;
        }

        private static IEnumerable<TreeNode<TSegment>> TreeNodesFromSegments(IEnumerable<IEnumerable<TSegment>> dataEntriesParts)
        {
            IList<TreeNode<TSegment>> nodes = new List<TreeNode<TSegment>>();

            foreach (IEnumerable<TSegment> entryParts in dataEntriesParts)
            {
                (IList<TreeNode<TSegment>> prefixChildren, TSegment[] entryPartsSuffix) = FindExistingPrefix(nodes, entryParts.ToArray());
                AppendSuffix(prefixChildren, entryPartsSuffix);
            }

            return nodes;
        }

        private static void AppendSuffix(IList<TreeNode<TSegment>> prefixChildren, TSegment[] entryPartsSuffix)
        {
            IList<TreeNode<TSegment>> currentChildren = prefixChildren;
            foreach (TSegment entryPart in entryPartsSuffix)
            {
                TreeNode<TSegment> entryPartNode = new(entryPart, new List<TreeNode<TSegment>>());
                currentChildren.Add(entryPartNode);
                currentChildren = entryPartNode.Children;
            }
        }

        private static (IList<TreeNode<TSegment>>, TSegment[]) FindExistingPrefix(IList<TreeNode<TSegment>> nodes, TSegment[] entryParts)
        {
            IList<TreeNode<TSegment>> currentChildren = nodes;

            int suffixIndex = 0;

            for (var i = 0; i < entryParts.Length; i++)
            {
                suffixIndex = i;
                var entryPart = entryParts[i];
                // kj2 optimize this select, and also the recursion. See also the todo about yield.
                if (currentChildren.Select(node => node.Value).Contains(entryPart))
                {
                    var childNode = currentChildren.Single(node => node.Value.Equals(entryPart));
                    currentChildren = childNode.Children;
                }
                else
                {
                    break;
                }
            }

            return (currentChildren, entryParts.Skip(suffixIndex).ToArray());
        }

        public IEnumerator<TPath> GetEnumerator() => Paths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}