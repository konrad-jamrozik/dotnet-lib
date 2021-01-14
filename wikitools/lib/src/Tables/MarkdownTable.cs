using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Tables
{
    public record MarkdownTable(ITabularData Data) : IWritableToText
    {
        public MarkdownTable(StringWriter sw) : this(TabularData(sw.GetStringBuilder().ToString())) { }

        public static TabularData TabularData(string str)
        {
            string[] lines = new TextLines(str).Value;

            var description = lines[0];
            // assert: lines[1] is an empty line.
            var headerRowLine = lines[2];
            // assert: lines[3] is HeaderDelimiterRow();
            // assert: line[-1] is an empty line;
            var rowLines = lines.Skip(4).SkipLast(1);

            var tabularData = new TabularData(
                Description: description,
                HeaderRow: UnwrapFromMarkdown(headerRowLine),
                Rows: rowLines.Select(UnwrapFromMarkdown).ToList()
            );
            return tabularData;

            List<object> UnwrapFromMarkdown(string line) =>
                line.Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(cell => cell.Trim())
                    .Cast<object>()
                    .Select(cell => int.TryParse((string) cell, out int cellInt) ? cellInt : cell)
                    .ToList();
        }

        public async Task WriteAsync(TextWriter textWriter)
        {
            var description   = await Data.GetDescription();
            var dataHeaderRow = Data.HeaderRow;
            var dataRows      = await Data.GetRows();

            var headerRow          = WrapInMarkdown(dataHeaderRow);
            var headerDelimiterRow = HeaderDelimiterRow();
            var rows               = dataRows.Select(WrapInMarkdown);

            var rowsToWrite = new List<string>
            {
                headerRow,
                headerDelimiterRow
            }.Union(rows);

            await textWriter.WriteLineAsync(description);
            await textWriter.WriteLineAsync();
            rowsToWrite.ToList().ForEach(async row => await textWriter.WriteLineAsync(row));
        }

        private string HeaderDelimiterRow()
            => string.Join("-", Enumerable.Repeat("|", Data.HeaderRow.Count + 1));

        private static string WrapInMarkdown(List<object> row)
            => row.Aggregate("|", (@out, col) => @out + " " + col + " |");
    }
}