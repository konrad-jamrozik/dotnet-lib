using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Data
{
    // kja to remove
    // // kja only way to enumerate: (depth, value)
    // // primary ctor: TreeData(treeNode)
    // // secondary ctors, inheriting:
    // // TreeData<entry, item>(rows, extract) : TreeData(buildTreeNode(rows, extract))
    // // FileTreeData ...  :  TreeData<string, string>(...)
    // // https://en.wikipedia.org/wiki/Trie
    // public record TreeData<TEntry, TItem>(IEnumerable<TEntry> Rows, Func<TEntry, IEnumerable<TItem>> Segments) : IEnumerable<TEntry> where TItem : IEquatable<TItem>
    // {
    //     public IEnumerator<TEntry> GetEnumerator() => Rows.GetEnumerator();
    //
    //     IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    // }

    public abstract record TreeData<TValue>(IEnumerable<TreeNode<TValue>> Nodes)
    {
        public IEnumerable<(int depth, TValue value)> AsPreorderEnumerable()
        {
            // kj2 instead of building the preorderTree at once, do yield. Also:
            // - the record ctor probably should have the raw tree data as ctor input,
            // and the serialized format should be one of the "From" method.
            //   - The From method probably should take lambda for splitting the input entries,
            //   possibly with well-known case for strings.
            return PreorderTraversal();
        }

        private IEnumerable<(int depth, TValue value)> PreorderTraversal()
        {
            // kj2 abstract this into generic preorder traversal algorithm
            IEnumerable<(int depth, TValue value)> preorderTree = PreorderTraversal(Nodes, depth: 0);
            return preorderTree;
        }

        private static IEnumerable<(int depth, TValue value)> PreorderTraversal(IEnumerable<TreeNode<TValue>> nodes, int depth) =>
            nodes.SelectMany(node => PreorderTraversal(node, depth)).ToList();

        private static IEnumerable<(int depth, TValue value)> PreorderTraversal(TreeNode<TValue> node, int depth)
        {
            var currNode = new List<(int depth, TValue value)> { (depth, node.Value) };
            var childNodes = node.Children.Any()
                ? PreorderTraversal(node.Children, depth + 1)
                : new List<(int depth, TValue value)>();
            return currNode.Concat(childNodes).ToList();
        }
    }
}