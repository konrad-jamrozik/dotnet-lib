using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Data;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Lib.Tests.Data
{
    public class FilePathTrieTests
    {
        [Fact] 
        public void TrieFromEmptyInput() => VerifyPreorderTraversal(
            new string[] {}, 
            new List<string[]>());

        [Fact] 
        public void TrieFromThreeSegmentPath() => VerifyPreorderTraversal(
            new[]
            {
                "foo\\bar\\baz"
            }, 
            new[]
            {
                "foo", "bar", "baz"
            });

        [Fact]
        public void TrieFromTwoSingleSegmentPaths() => VerifyPreorderTraversal(
            new[]
            {
                "foo", 
                "bar"
            }, 
            new[]
            {
                new[] { "foo" }, 
                new[] { "bar" },
            });

        [Fact]
        public void TrieFromRepeatedPaths() => VerifyPreorderTraversal(
            new[]
            {
                "foo\\bar", 
                "foo\\bar"
            }, 
            new[]
            {
                new[] { "foo", "bar" }
            });

        [Fact]
        public void TrieFromReversedPaths() => VerifyPreorderTraversal(
            new[]
            {
                "foo\\bar", 
                "bar\\foo"
            }, 
            new[] 
            { 
                new[] { "foo", "bar" }, 
                new[] { "bar", "foo" }
            });

        // kja 3 the expectations here are wrong. Each prefixs should be included, as each wiki prefix
        // path can have visits, and that's what we need.
        // Also, I am not sure if traversing file system paths will give us this case when it comes
        // to inputs, or just "foo\\bar\\baz".
        // See how Wikitools.Lib.OS.FileTree.FilePathTrie works to check this.
        [Fact]
        public void TrieFromStaircasePaths() => VerifyPreorderTraversal(
            new[]
            {
                "foo", 
                "foo\\bar",
                "foo\\bar\\baz"
            }, 
            new[]
            {
                new[] { "foo", "bar", "baz" }
            });

        [Fact]
        public void TrieFromReverseStaircasePaths() => VerifyPreorderTraversal(
            new[]
            {
                "foo\\bar\\baz",
                "foo\\bar",
                "foo", 
            }, 
            new[]
            {
                new[] { "foo", "bar", "baz" }
            });

        [Fact]
        public void TrieFromBranchingPaths() => VerifyPreorderTraversal(
            new[] 
            { 
                "foo\\bar1", 
                "foo\\bar2"
            },
            new[]
            {
                new[] { "foo" }, 
                new[] { "foo", "bar1" }, 
                new[] { "foo", "bar2" }
            });

        [Fact]
        public void TrieWithEmptyPathsSegmentsWhenComputingSuffixes() => VerifyPreorderTraversal(
            new[] { 
                "foo\\bar", 
                "foo\\baz",
                "foo" },
            new[]
            {
                new[] { "foo" }, 
                new[] { "foo", "bar" }, 
                new[] { "foo", "baz" }, 
            });

        [Fact]
        public void TrieFromDoublyBranchingPaths() => VerifyPreorderTraversal(
            new[] { 
                "foo\\bar2\\baz1",
                "foo\\baz1\\foo",
                "foo\\bar2\\baz2\\qux",
                "foo\\bar1\\baz1", 
                "foo\\bar1\\baz2\\quux"
            },
            new[]
            {
                new[] { "foo" }, 
                new[] { "foo", "bar2" }, 
                new[] { "foo", "bar2", "baz1" }, 
                new[] { "foo", "bar2", "baz2", "qux" },
                new[] { "foo", "baz1", "foo" }, 
                new[] { "foo", "bar1" }, 
                new[] { "foo", "bar1", "baz1" }, 
                new[] { "foo", "bar1", "baz2", "quux" }, 
            });

        [Fact]
        public void TrieFromJaggedPaths() => VerifyPreorderTraversal(
            new[] { 
                "foo\\bar\\baz\\qux\\quux", 
                "bar",
                "foo\\bar\\baz",
                "foo\\bar\\bar",
                "foo" },
            new[]
            {
                new[] { "foo", "bar" }, 
                new[] { "foo", "bar", "baz", "qux", "quux" }, 
                new[] { "foo", "bar", "bar" }, 
                new[] { "bar" }, 
            });


        private static void VerifyPreorderTraversal(string[] pathsUT, string[] expectedPathSegments)
            => VerifyPreorderTraversal(pathsUT, expectedPathSegments.InList());

        private static void VerifyPreorderTraversal(string[] pathsUT, IList<string[]> expectedPathsSegments)
        {
            var trieUT = new FilePathTrie(pathsUT);
            
            // Act
            var preorderTraversalUT = trieUT.PreorderTraversal();

            // Filter out Suffixes by calling PathPart.Leaf, as we don't test for correctness of suffixes.
            PathPart<object?>[] expectedPaths = expectedPathsSegments.Select(PathPart.Leaf).ToArray();
            PathPart<object?>[] actualPaths = preorderTraversalUT.Select(path => PathPart.Leaf(path.Segments)).ToArray();

            expectedPaths.Zip(actualPaths).Assert(
                pathsPair => pathsPair.First == pathsPair.Second,
                pathsPair => new Exception($"expected: {pathsPair.First} actual: {pathsPair.Second}")).Consume();
            Assert.Equal(expectedPaths.Length, actualPaths.Length);
        }
    }
}