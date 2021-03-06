using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Data
{
    // kja design the new TreeData, based on my Dendrond projects.wikitools.scratchpad

    public abstract record TreeData<TValue>(IEnumerable<TreeNode<TValue>> Nodes)
    {
        public IEnumerable<(int depth, TValue value)> AsPreorderEnumerable() => PreorderTraversal();

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