using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Data;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Lib.Tests.Data
{
    // kja curr work
    public class TrieFromPathsTests
    {
        [Fact]
        public void TestTreeData1() =>
            Verify2(
                data: new[] { "foo\\bar" }, 
                expectedSegments: new[] { "foo", "bar" }.WrapInList());

        // kja fails, as the empty prefix is not filtered out
        [Fact]
        public void TestTreeData11() =>
            Verify2(
                data: new[] { "foo", "bar" }, 
                expectedSegments: new[]
                {
                    new[] { "foo" }, 
                    new[] { "bar" },
                });

        [Fact]
        public void TestTreeData2() =>
            Verify2(
                data: new[] { 
                    "foo\\bar1", 
                    "foo\\bar2" },
                expectedSegments: new[]
                {
                    new[] { "foo" }, 
                    new[] { "foo", "bar1" }, 
                    new[] { "foo", "bar2" }
                });

        [Fact]
        public void TestTreeData31() =>
            Verify2(
                data: new[] { 
                    "foo\\bar2\\baz1",
                    "foo\\baz1\\foo",
                    "foo\\bar2\\baz2",
                    "foo\\bar1\\baz1", 
                    "foo\\bar1\\baz2"
                },
                expectedSegments: new[]
                {
                    new[] { "foo" }, 
                    new[] { "foo", "bar2" }, 
                    new[] { "foo", "bar2", "baz1" }, 
                    new[] { "foo", "bar2", "baz2" },
                    new[] { "foo", "baz1", "foo" }, 
                    new[] { "foo", "bar1" }, 
                    new[] { "foo", "bar1", "baz1" }, 
                    new[] { "foo", "bar1", "baz2" }, 
                });


        private static void Verify2(string[] data, IList<string[]> expectedSegments)
        {
            var trie = new TrieFromPaths(data, FilePathTreeData.SplitPath);
            PathPart<object?>[] expectedPathsParts = expectedSegments.Select(PathPart.Leaf).ToArray();
            
            // Act
            var preorderTraversal = trie.PreorderTraversal();

            // Erase Suffixes by calling PathPart.Leaf, as we don't test for that.
            PathPart<object?>[] paths = preorderTraversal.Select(path => PathPart.Leaf(path.Segments)).ToArray();

            expectedPathsParts.Zip(paths).Assert(
                tuple => tuple.First == tuple.Second,
                tuple => new Exception($"expected: {tuple.First} actual: {tuple.Second}")).Consume();
            Assert.Equal(expectedPathsParts.Length, paths.Length);
        }


        [Fact]
        public void TestTreeData4()
        {
            var treeData = new TrieFromPaths(new[]
            {
                "qux1\\foo\\bar1", 
                "qux1\\foo\\bar2",
                "qux2\\foo\\bar1",
                "qux2\\foo\\bar2",
                "qux2\\bar1\\foo",
                "qux2\\foo\\bar3",
                "qux2\\foo\\bar3\\bar2",
            }, FilePathTreeData.SplitPath);
            var expectedRows = new (int depth, string)[]
            {
                (0, "qux1"),
                (1, "foo"),
                (2, "bar1"),
                (2, "bar2"),
                (0, "qux2"),
                (1, "foo"),
                (2, "bar1"),
                (2, "bar2"),
                (2, "bar3"),
                (3, "bar2"),
                (1, "bar1"),
                (2, "foo")
            };

            Verify(treeData, expectedRows);
        }

        private static void Verify(Trie<object?> trie, (int depth, string path)[] expectedRows)
        {
            // Act
            PathPart<object?>[] paths = trie.PreorderTraversal().ToArray();

            Assert.Equal(expectedRows.Length, paths.Length);

            MoreEnumerable.ForEach(
                expectedRows.Zip(paths),
                entry =>
                {
                    var pathPart = entry.Second;
                    var segments = pathPart.Segments.ToList();
                    (int depth, string segment) actual = (segments.Count - 1, string.Join(Path.DirectorySeparatorChar, segments));
                    // kja problem here: if the preorder returns just the leaf from each path, the depth is not computed and 
                    // cannot be derives. Also the type of "segments[]" doesn't make much sense, as there is always 1.
                    // better to return entire prefix from the abstract Trie.
                    //Assert.Equal(entry.First, actual);
                    Assert.Equal(entry.First.path, actual.segment);
                });
        }
    }
}