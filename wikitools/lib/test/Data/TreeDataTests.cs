using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wikitools.Lib.Data;
using Xunit;
using Xunit.Abstractions;

namespace Wikitools.Lib.Tests.Data
{
    public class TreeDataTests
    {
        private readonly ITestOutputHelper output;

        public TreeDataTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        
        [Fact]
        public void TestTreeData1()
        {
            var treeData = new FilePathTreeData(new[]
            {
                "foo\\bar1", 
                "foo\\bar2"
            });
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
            var treeData = new FilePathTreeData(new[]
            {
                "foo\\bar1\\baz1", 
                "foo\\bar1\\baz2",
                "foo\\bar2\\baz1", 
                "foo\\bar2\\baz2",
            });
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
            var treeData = new FilePathTreeData(new[]
            {
                "qux1\\foo\\bar1", 
                "qux1\\foo\\bar2",
                "qux2\\foo\\bar1",
                "qux2\\foo\\bar2",
                "qux2\\bar1\\foo",
                "qux2\\foo\\bar3",
                "qux2\\foo\\bar3\\bar2",
            });
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
                (2, "foo"),
            };

            Verify(treeData, expectedRows);
        }

        private static void Verify(FilePathTreeData treeData, (int depth, string)[] expectedRows)
        {
            // Act
            (int depth, string value)[] rows = treeData.AsPreorderEnumerable().ToArray();

            // kja this assert is busted, as it doesn't account for expected having more elems.
            // kj2 introduce abstraction for this
            expectedRows.Zip(rows).ToList().ForEach(entry => Assert.Equal(entry.First, entry.Second));
        }
    }
}