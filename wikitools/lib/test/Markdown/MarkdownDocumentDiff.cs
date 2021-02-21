using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Tests.Json;

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

        private static async Task AssertNoDiffBetween(MarkdownDocument expected, Task<ParsedMarkdownDocument> actual) =>
            new JsonDiffAssertion(await expected.Content, await (await actual).Content).Assert();
    }
}