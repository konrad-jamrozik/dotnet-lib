using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wikitools.Lib.Data
{
    // https://en.wikipedia.org/wiki/Trie
    public record TreeData<T>(IEnumerable<T> Data) : IEnumerable<T> where T : IEquatable<T>
    {
        // kja curr work
        private List<(int depth, T)> BuildPreorderTree(IEnumerable<T> data)
        {
            List<TreeNode> nodes = new();

            // kj2 the ToString()!.Split() should be a lambda param and .Cast<T> should be removed.
            // Basically the input should be an enumerable of "T == array of U", and a lambda function
            // telling how to convert T to array of U.
            // Maybe have dedicated method that it will recognize that if T is string, then U is also string, and the conversion
            // logic from T to U[] is the Splitting currently hardcoded here.
            List<T[]> dataEntriesParts = data.Select(entry => entry.ToString()!.Split(Path.DirectorySeparatorChar).Cast<T>().ToArray()).ToList();

            foreach (T[] entryParts in dataEntriesParts)
            {
                (List<TreeNode> prefixChildren, T[] entryPartsSuffix) = FindExistingPrefix(nodes, entryParts);
                AppendSuffix(prefixChildren, entryPartsSuffix);
            }

            // kj2 abstract this into generic preorder traversal algorithm
            List<(int depth, T)> preorderTree = TreeNodesToPreorderTree(nodes, depth: 0);
            return preorderTree;
        }

        private List<(int depth, T)> TreeNodesToPreorderTree(List<TreeNode> nodes, int depth) =>
            nodes.SelectMany(node => TreeNodeToPreorderTree(node, depth)).ToList();

        private List<(int depth, T)> TreeNodeToPreorderTree(TreeNode node, int depth)
        {
            var currNode = new List<(int depth, T)> { (depth, node.Value) };
            var childNodes = node.Children.Any()
                ? TreeNodesToPreorderTree(node.Children, depth + 1)
                : new List<(int depth, T)>();
            return currNode.Concat(childNodes).ToList();
        }

        private void AppendSuffix(List<TreeNode> prefixChildren, T[] entryPartsSuffix)
        {
            List<TreeNode> currentChildren = prefixChildren;
            foreach (T entryPart in entryPartsSuffix)
            {
                var entryPartNode = new TreeNode(entryPart, new List<TreeNode>());
                currentChildren.Add(entryPartNode);
                currentChildren = entryPartNode.Children;
            }
        }

        private (List<TreeNode>, T[]) FindExistingPrefix(List<TreeNode> nodes, T[] entryParts)
        {
            List<TreeNode> currentChildren = nodes;

            int suffixIndex = 0;

            for (var i = 0; i < entryParts.Length; i++)
            {
                suffixIndex = i;
                var entryPart = entryParts[i];
                if (currentChildren.Select(node => node.Value).Contains(entryPart))
                {
                    var childNode = nodes.Single(node => node.Value.Equals(entryPart));
                    currentChildren = childNode.Children;
                }
                else
                {
                    break;
                }
            }

            return (currentChildren, entryParts.Skip(suffixIndex).ToArray());
        }

        public IEnumerable<(int depth, T)> AsPreorderEnumerable()
        {
            List<(int depth, T)> preorderTree = BuildPreorderTree(Data);

            // kj2 instead of building the preorderTree at once, do yield. Also:
            // - the record ctor probably should have the raw tree data as ctor input,
            // and the serialized format should be one of the "From" method.
            //   - The From method probably should take lambda for splitting the input entries,
            //   possibly with well-known case for strings.
            return preorderTree;
        }

        public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private record TreeNode(T Value, List<TreeNode> Children);

        /* kja this record should implement generic algorithm on Tree-like data structures.
        That allows to project into nested structure.

        Namely, entries like:
        foo/bar1/baz1
        foo/bar1/baz2
        foo/bar2/baz1
        foo/bar2/baz1
        should be projectable to:
        foo
          bar1
            baz1
            baz2
          bar2
            baz3
            baz4

        It should be possible to provide the input entries as a series of leafs with separators,
        as in the example above. 
        However, internally, the record probably should keep the data efficiently, via a tree.

        */
    }
}