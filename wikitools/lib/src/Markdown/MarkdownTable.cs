using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Tables;

namespace Wikitools.Lib.Markdown
{
    internal record MarkdownTable(TabularData Table)
    {
        public MarkdownTable(string[] markdownLines) : this(UnwrapFromMarkdown(markdownLines)) { }

        public override string ToString()
        {
            var headerRow          = WrapInMarkdown(Table.HeaderRow);
            var headerDelimiterRow = HeaderDelimiterRow(Table.HeaderRow);
            var rows               = Table.Rows.Select(WrapInMarkdown);

            var rowsToWrite = new List<string>
            {
                headerRow,
                headerDelimiterRow
            }.Union(rows);

            return string.Join(Environment.NewLine, rowsToWrite);
        }

        private static TabularData UnwrapFromMarkdown(string[] lines) =>
            new(
                (
                    headerRow: UnwrapFromMarkdown(lines[0]),
                    rows: lines.Skip(2).Select(UnwrapFromMarkdown).ToArray()
                )
            );

        private static string HeaderDelimiterRow(object[] headerRow)
            => string.Join("-", Enumerable.Repeat("|", headerRow.Length + 1));

        private static string WrapInMarkdown(object[] row)
            => row.Aggregate("|", (@out, col) => @out + " " + col + " |");

        private static object[] UnwrapFromMarkdown(string markdownLine) =>
            markdownLine.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(cell => cell.Trim())
                .Cast<object>()
                .Select(cell => int.TryParse((string) cell, out int cellInt) ? cellInt : cell)
                .ToArray();
    }
}