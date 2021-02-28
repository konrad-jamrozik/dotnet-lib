using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wikitools.Lib.Data;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Markdown
{
    public record MarkdownDocument(Task<object[]> Content) : IWritableToText
    {
        public async Task WriteAsync(TextWriter textWriter) => await textWriter.WriteAsync(await ToMarkdown(this));

        private static async Task<string> ToMarkdown(MarkdownDocument doc) =>
            (await doc.Content).Select(entry => entry switch
            {
                TabularData td => new MarkdownTable(td) + Environment.NewLine,
                _ => entry + Environment.NewLine
            })
            .Aggregate(new StringBuilder(), (sb, str) => sb.Append(str))
            .ToString();
    }
}