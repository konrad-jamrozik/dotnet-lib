using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wikitools.Lib.Tables;

namespace Wikitools.Lib.Markdown
{
    public abstract record MarkdownDocument : IWritableToText
    {
        public async Task WriteAsync(TextWriter textWriter) => await textWriter.WriteAsync(ToMarkdown(this));

        public override string ToString() => ToMarkdown(this);

        public abstract List<object> Content { get; }

        private static string ToMarkdown(MarkdownDocument doc) =>
            doc.Content
                .Select(entry => entry switch
                {
                    // @formatter:off
                    TabularData2 td => new MarkdownTable2(td) + Environment.NewLine,
                    _               => entry + Environment.NewLine
                    // @formatter:on
                })
                .Aggregate(new StringBuilder(), (sb, str) => sb.Append(str))
                .ToString();

        private record MarkdownTable2(TabularData2 Table)
        {
            public override string ToString()
            {
                var headerRow          = WrapInMarkdown(Table.Data.headerRow);
                var headerDelimiterRow = HeaderDelimiterRow(Table.Data.headerRow);
                var rows               = Table.Data.rows.Select(WrapInMarkdown);

                var rowsToWrite = new List<string>
                {
                    headerRow,
                    headerDelimiterRow
                }.Union(rows);

                return string.Join(Environment.NewLine, rowsToWrite);
            }

            private static string HeaderDelimiterRow(object[] headerRow)
                => string.Join("-", Enumerable.Repeat("|", headerRow.Length + 1));

            private static string WrapInMarkdown(object[] row)
                => row.Aggregate("|", (@out, col) => @out + " " + col + " |");
        }
    }
}