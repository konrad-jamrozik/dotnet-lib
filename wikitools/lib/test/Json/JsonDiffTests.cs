using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Wikitools.Lib.Json;
using Xunit;
using Xunit.Abstractions;

namespace Wikitools.Lib.Tests.Json
{
    public class JsonDiffTests
    {
        private readonly ITestOutputHelper output;

        public JsonDiffTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void DiffsAddedProperty()
        {
            var json1 = JsonDocument.Parse("{}");
            var json2 = JsonDocument.Parse(@"{ 'a': 5 }".Replace('\'','"'));
            var diff = new JsonDiff(json1, json2);
            Assert.Equal("+", diff.JsonElement.GetProperty("a").GetString());
            output.WriteLine(diff.ToString());
            
        }
    }
}
