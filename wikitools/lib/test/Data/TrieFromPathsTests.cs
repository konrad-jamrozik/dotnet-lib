using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Data;
using Xunit;

namespace Wikitools.Lib.Tests.Data
{
    // kja curr work
    public class TrieFromPathsTests
    {
        [Fact]
        public void TestTreeData1()
        {
            var treeData = new TrieFromPaths(new[]
            {
                "foo\\bar1", 
                "foo\\bar2"
            }, FilePathTreeData.SplitPath);
            var expectedRows = new (int depth, string)[]
            {
                (0, "foo"),
                (1, "bar1"),
                (1, "bar2")
            };

            Verify(treeData, expectedRows);
        }

        [Fact]
        public void TestTreeData2()
        {
            var treeData = new TrieFromPaths(new[]
            {
                "foo\\bar1\\baz1", 
                "foo\\bar1\\baz2",
                "foo\\bar2\\baz1", 
                "foo\\bar2\\baz2",
            }, FilePathTreeData.SplitPath);
            var expectedRows = new (int depth, string)[]
            {
                (0, "foo"),
                (1, "bar1"),
                (2, "baz1"),
                (2, "baz2"),
                (1, "bar2"),
                (2, "baz1"),
                (2, "baz2")
            };
            Verify(treeData, expectedRows);
        }

        [Fact]
        public void TestTreeData3()
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
                (1, "bar1")
            };

            Verify(treeData, expectedRows);
        }


        private static void Verify(Trie<object?> trie, (int depth, string path)[] expectedRows)
        {
            // Act
            PathPart<object?>[] paths = trie.PreorderTraversal().ToArray();

            Assert.Equal(expectedRows.Length, paths.Length);

            expectedRows.Zip(paths).ForEach(entry =>
                {
                    var pathPart = entry.Second;
                    var segments = pathPart.Segments.ToList();
                    (int depth, object) actual = (segments.Count, string.Join(Path.DirectorySeparatorChar, segments));
                    Assert.Equal(entry.First, actual);
                });
        }
    }
}