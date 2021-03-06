﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Data
{
    public record TreeDataFromPaths<TLeaf, TSegment>
        (IEnumerable<TLeaf> Rows, Func<TLeaf, IEnumerable<TSegment>> Segments) : TreeData<TSegment>(
            TreeNodes(Rows, Segments)) where TSegment : IEquatable<TSegment>
    {
        private static IEnumerable<TreeNode<TSegment>> TreeNodes(
            IEnumerable<TLeaf> paths,
            Func<TLeaf, IEnumerable<TSegment>> segments)
        {
            IEnumerable<IEnumerable<TSegment>> pathsSegments = paths.Select(segments);
            IEnumerable<TreeNode<TSegment>> nodes = TreeNodesFromItems(pathsSegments);
            return nodes;
        }

        private static IEnumerable<TreeNode<TSegment>> TreeNodesFromItems(IEnumerable<IEnumerable<TSegment>> dataEntriesParts)
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
    }
}