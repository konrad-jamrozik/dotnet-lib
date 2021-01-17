using System;
using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Json;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Tables;
using Xunit;

namespace Wikitools.Lib.Tests.Tables
{
    // kja get rid of all extension classes.
    public static class TabularDataAssertionExtensions
    {
        public static async Task Verify(TabularData2 expected, MarkdownDocument sut) =>
            AssertNoDiffBetween(expected, await Act(sut));

        private static async Task<TabularData> Act(MarkdownDocument sut)
        {
            // Arrange output sink
            await using var sw = new StringWriter();

            // Act
            await sut.WriteAsync(sw);

            // kja NEXT need to read markdown table here
            return (TabularData) new MarkdownTable(sw).Data;
        }

        private static void AssertNoDiffBetween(TabularData2 expected, TabularData actual)
        {
            var jsonDiff = new JsonDiff(expected, actual);
            Assert.True(jsonDiff.IsEmpty,
                $"The expected baseline is different than actual target. Diff:{Environment.NewLine}{jsonDiff}");
        }
    }
}