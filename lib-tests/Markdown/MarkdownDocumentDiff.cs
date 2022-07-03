using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Tests.Json;

namespace Wikitools.Lib.Tests.Markdown;

public record MarkdownDocumentDiff(MarkdownDocument Expected, MarkdownDocument Sut)
{
    public Task Verify() => 
        AssertNoDiffBetween(Expected, Act(Sut));

    private static async Task AssertNoDiffBetween(MarkdownDocument expected, Task<MarkdownDocument> actual)
    {
        var expectedContent = await expected.Content;
        var actualContent = await (await actual).Content;
        new JsonDiffAssertion(expectedContent, actualContent).Assert();
    }

    private static async Task<MarkdownDocument> Act(MarkdownDocument sut)
    {
        await using var sw = new StringWriter();

        // Act
        await sut.WriteAsync(sw);

        return new ParsedMarkdownDocument(sw);
    }
}