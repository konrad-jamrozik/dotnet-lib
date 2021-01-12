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
        // kja write more tests
        [Fact]
        public void DiffsAddedProperty()
        {
            var baseline = JsonDocument.Parse("{}");
            var target = JsonDocument.Parse(@"{ 'a': 5 }".Replace('\'','"'));
            var diff = new JsonDiff(baseline, target);

            Assert.Equal(@"{""a"":""+""}", diff.ToRawString());
            Assert.Equal("+", diff.JsonElement.GetProperty("a").GetString());
            
            
        }
    }
}
