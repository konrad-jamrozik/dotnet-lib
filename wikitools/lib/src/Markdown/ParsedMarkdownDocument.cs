using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Primitives;
using static MoreLinq.MoreEnumerable;

namespace Wikitools.Lib.Markdown
{
    public record ParsedMarkdownDocument : MarkdownDocument
    {
        public ParsedMarkdownDocument(StringWriter sw) : base(GetContent(sw)) { }

        private static object[] GetContent(StringWriter sw)
        {
            string[] markdownLines = new TextLines(sw.GetStringBuilder().ToString()).SplitTrimmingEnd;

            var groups = markdownLines.GroupAdjacent(line => line.FirstOrDefault() == '|');

            var content = groups.SelectMany(group =>
            {
                var table = Return(new MarkdownTable2(@group.ToArray()).Table).Cast<object>();
                return @group.Key ? table : @group.ToArray();
            }).ToArray();

            return content;
        }
    }
}