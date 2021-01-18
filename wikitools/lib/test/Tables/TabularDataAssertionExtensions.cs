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
        public static async Task Verify(MarkdownDocument expected, MarkdownDocument sut) =>
            AssertNoDiffBetween(expected, await Act(sut));

        private static async Task<MarkdownDocument> Act(MarkdownDocument sut)
        {
            // Arrange output sink
            await using var sw = new StringWriter();

            // Act
            await sut.WriteAsync(sw);

            return new ParsedMarkdownDocument(sw);
        }

        private static void AssertNoDiffBetween(MarkdownDocument expected, MarkdownDocument actual)
        {
            // kja will this just work on MarkdownDocument? It should! It is a record after all.
            var jsonDiff = new JsonDiff(expected, actual);
            Assert.True(jsonDiff.IsEmpty,
                $"The expected baseline is different than actual target. Diff:{Environment.NewLine}{jsonDiff}");
        }
    }
}