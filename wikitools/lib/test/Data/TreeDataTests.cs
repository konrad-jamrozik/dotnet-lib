using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        // kja curr test
        [Fact]
        public void TestTreeData()
        {
            var treeData = new TreeData<string>(new[] { "foo\\bar1", "foo\\bar2" });
            foreach ((int depth, string s) entry in treeData.AsPreorderEnumerable())
            {
                
            }
        }

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