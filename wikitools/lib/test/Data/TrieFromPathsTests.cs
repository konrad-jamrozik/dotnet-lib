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
    public class TrieFromPathsTests
    {
        [Fact] 
        public void TrieFromEmptyInput() => Verify(
            new string[] {}, 
            new List<string[]>());

        [Fact] public void TrieFromTwoSegmentPath() => Verify(
            new[]
            {
                "foo\\bar"
            }, 
            new[]
            {
                "foo", "bar"
            });

        [Fact]
        public void TrieFromTwoSingleSegmentPaths() => Verify(
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
        public void TrieFromRepeatedPaths() => Verify(
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
        public void TrieFromReversedPaths() => Verify(
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

        [Fact]
        public void TrieFromStaircasePaths() => Verify(
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
        public void TrieFromReverseStaircasePaths() => Verify(
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
        public void TrieFromBranchingPaths() => Verify(
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
        public void TrieWithEmptyPathsSegmentsWhenComputingSuffixes() => Verify(
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
        public void TrieFromDoublyBranchingPaths() => Verify(
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
        public void TrieFromJaggedPaths() => Verify(
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


        private static void Verify(string[] pathsData, string[] expectedSegment)
            => Verify(pathsData, expectedSegment.WrapInList());

        private static void Verify(string[] pathsData, IList<string[]> expectedSegments)
        {
            var trie = new TrieFromPaths(pathsData, FileSystem.SplitPath);
            PathPart<object?>[] expectedPaths = expectedSegments.Select(PathPart.Leaf).ToArray();
            
            // Act
            var preorderTraversal = trie.PreorderTraversal();

            // Erase Suffixes by calling PathPart.Leaf, as we don't test for that.
            PathPart<object?>[] paths = preorderTraversal.Select(path => PathPart.Leaf(path.Segments)).ToArray();

            expectedPaths.Zip(paths).Assert(
                tuple => tuple.First == tuple.Second,
                tuple => new Exception($"expected: {tuple.First} actual: {tuple.Second}")).Consume();
            Assert.Equal(expectedPaths.Length, paths.Length);
        }
    }
}