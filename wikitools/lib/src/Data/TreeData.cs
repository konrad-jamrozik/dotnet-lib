using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Data
{
    // kja only way to enumerate: (depth, value)
    // primary ctor: TreeData(treeNode)
    // secondary ctors, inheriting:
    // TreeData<entry, item>(rows, extract) : TreeData(buildTreeNode(rows, extract))
    // FileTreeData ...  :  TreeData<string, string>(...)
    // https://en.wikipedia.org/wiki/Trie
    public record TreeData<TEntry, TItem>(IEnumerable<TEntry> Rows, Func<TEntry, IEnumerable<TItem>> ExtractItems) : IEnumerable<TEntry> where TItem : IEquatable<TItem>
    {
        public IEnumerable<(int depth, TItem value)> AsPreorderEnumerable()
        {
            IEnumerable<IEnumerable<TItem>> itemsColl = Rows.Select(ExtractItems);

            // kj2 instead of building the preorderTree at once, do yield. Also:
            // - the record ctor probably should have the raw tree data as ctor input,
            // and the serialized format should be one of the "From" method.
            //   - The From method probably should take lambda for splitting the input entries,
            //   possibly with well-known case for strings.
            IEnumerable<(int depth, TItem value)> buildPreorderTree2 = BuildPreorderTree2(itemsColl);

            return buildPreorderTree2;
        }

        private static List<(int depth, TItem value)> BuildPreorderTree2(IEnumerable<IEnumerable<TItem>> dataEntriesParts)
        {
            List<TreeNode> nodes = new();

            foreach (IEnumerable<TItem> entryParts in dataEntriesParts)
            {
                (List<TreeNode> prefixChildren, TItem[] entryPartsSuffix) = FindExistingPrefix(nodes, entryParts.ToArray());
                AppendSuffix(prefixChildren, entryPartsSuffix);
            }

            // kj2 abstract this into generic preorder traversal algorithm
            List<(int depth, TItem value)> preorderTree = TreeNodesToPreorderTree(nodes, depth: 0);
            return preorderTree;
        }

        private static List<(int depth, TItem value)> TreeNodesToPreorderTree(List<TreeNode> nodes, int depth) =>
            nodes.SelectMany(node => TreeNodeToPreorderTree(node, depth)).ToList();

        private static List<(int depth, TItem value)> TreeNodeToPreorderTree(TreeNode node, int depth)
        {
            var currNode = new List<(int depth, TItem value)> { (depth, node.Value) };
            var childNodes = node.Children.Any()
                ? TreeNodesToPreorderTree(node.Children, depth + 1)
                : new List<(int depth, TItem value)>();
            return currNode.Concat(childNodes).ToList();
        }

        private static void AppendSuffix(List<TreeNode> prefixChildren, TItem[] entryPartsSuffix)
        {
            List<TreeNode> currentChildren = prefixChildren;
            foreach (TItem entryPart in entryPartsSuffix)
            {
                var entryPartNode = new TreeNode(entryPart, new List<TreeNode>());
                currentChildren.Add(entryPartNode);
                currentChildren = entryPartNode.Children;
            }
        }

        private static (List<TreeNode>, TItem[]) FindExistingPrefix(List<TreeNode> nodes, TItem[] entryParts)
        {
            List<TreeNode> currentChildren = nodes;

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

        public IEnumerator<TEntry> GetEnumerator() => Rows.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private record TreeNode(TItem Value, List<TreeNode> Children);
    }
}