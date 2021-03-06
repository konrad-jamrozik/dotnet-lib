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
                (1, "bar1")
            };

            Verify(treeData, expectedRows);
        }

        private static void Verify(FilePathTreeData treeData, (int depth, string)[] expectedRows)
        {
            // Act
            (int depth, string value)[] rows = treeData.AsPreorderEnumerable().ToArray();

            // kj2 introduce abstraction for this
            expectedRows.Zip(rows).ToList().ForEach(entry => Assert.Equal(entry.First, entry.Second));
        }

        // kj2 to remove
        [Fact]
        public async Task TaskTests()
        {

            // var task = Task.Factory.StartNew(() =>
            // {
            //     output.WriteLine("Ungh");
            //     return "bums";
            // });
            //
            // var task2 = MyMethod();

            // var task = new Task<string>(() =>
            // {
            //     output.WriteLine("stdout");
            //     return "bums";
            // });
            var stringBuilder = new StringBuilder();
            var ret = MyMethod(stringBuilder);
            output.WriteLine(stringBuilder.ToString());

            output.WriteLine("reached end");
            await Task.Delay(100);
            //var str = await task;
        }

        public async Task<string> MyMethod(StringBuilder stringBuilder)
        {
            TextWriter tw = new StringWriter(stringBuilder);
            output.WriteLine("delay start");
            await Task.Delay(300);
            output.WriteLine("delay end");
            await tw.WriteAsync("bums");
            return "bla";
        }
    }
}