using System;
using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Json;
using Wikitools.Lib.Markdown;
using Xunit;

namespace Wikitools.Lib.Tests.Markdown
{
    public record MarkdownDocumentDiff(MarkdownDocument Expected, MarkdownDocument Sut)
    {
        public Task Verify() => 
            AssertNoDiffBetween(Expected, Act(Sut));

        private static async Task<ParsedMarkdownDocument> Act(MarkdownDocument sut)
        {
            await using var sw = new StringWriter();

            // Act
            await sut.WriteAsync(sw);

            return new ParsedMarkdownDocument(sw);
        }

        private static async Task AssertNoDiffBetween(MarkdownDocument expected, Task<ParsedMarkdownDocument> actual)
        {
            var jsonDiff = new JsonDiff(await expected.Content, await (await actual).Content);
            Assert.True(jsonDiff.IsEmpty,
                $"The expected baseline is different than actual target. Diff:{Environment.NewLine}{jsonDiff}");
        }
    }
}