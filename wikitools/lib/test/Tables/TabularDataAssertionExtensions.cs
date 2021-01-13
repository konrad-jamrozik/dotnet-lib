using System;
using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Json;
using Wikitools.Lib.Tables;
using Xunit;

namespace Wikitools.Lib.Tests.Tables
{
    public static class TabularDataAssertionExtensions
    {
        public static async Task Verify(TabularData expected, ITabularData sut) =>
            AssertNoDiffBetween(expected, await Act(sut));

        private static async Task<TabularData> Act(ITabularData sut)
        {
            // Arrange output sink
            await using var sw = new StringWriter();

            // Act
            await new MarkdownTable(sut).WriteAsync(sw);

            return (TabularData) new MarkdownTable(sw).Data;
        }

        private static void AssertNoDiffBetween(TabularData expected, TabularData actual)
        {
            var jsonDiff = new JsonDiff(expected, actual);
            Assert.True(jsonDiff.IsEmpty,
                $"The expected baseline is different than actual target. Diff:{Environment.NewLine}{jsonDiff}");
        }
    }
}