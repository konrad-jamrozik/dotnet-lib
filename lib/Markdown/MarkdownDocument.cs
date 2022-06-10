using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wikitools.Lib.Data;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Markdown;

public record MarkdownDocument(Task<object[]> Content) : IWritableToText
{
    // kj2-report needs to be duplicated a lot in UT assertions. Fix.
    // kj2-json JsonDiff doesn't seem to properly show the extra whitespace on assertion failures.
    public const string LineBreakMarker = "  ";

    protected MarkdownDocument(object[] content) : this(Task.FromResult(content)) 
    { }

    public async Task WriteAsync(TextWriter textWriter)
        => await textWriter.WriteAsync(await ToMarkdown(this));

    private static async Task<string> ToMarkdown(MarkdownDocument doc) =>
        (await doc.Content).Select(entry => entry switch
        {
            TabularData td => new MarkdownTable(td) + Environment.NewLine,
            _ => entry + LineBreakMarker + Environment.NewLine
        })
        .Aggregate(new StringBuilder(), (sb, str) => sb.Append(str))
        .ToString();
}