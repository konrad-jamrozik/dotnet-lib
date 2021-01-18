using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wikitools.Lib.Tables;

namespace Wikitools.Lib.Markdown
{
    public record MarkdownDocument(object[] Content) : IWritableToText
    {
        public async Task WriteAsync(TextWriter textWriter) => await textWriter.WriteAsync(ToMarkdown(this));

        public override string ToString() => ToMarkdown(this);

        private static string ToMarkdown(MarkdownDocument doc) =>
            doc.Content
                .Select(entry => entry switch
                {
                    // @formatter:off
                    TabularData td => new MarkdownTable(td) + Environment.NewLine,
                    _               => entry + Environment.NewLine
                    // @formatter:on
                })
                .Aggregate(new StringBuilder(), (sb, str) => sb.Append(str))
                .ToString();
    }
}