using System;
using System.IO;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Markdown
{
    public record ParsedMarkdownDocument : MarkdownDocument
    {
        public ParsedMarkdownDocument(StringWriter sw) : base(GetContent(sw)) { }

        private static MarkdownDocument GetContent(StringWriter sw)
        {
            string[] lines = new TextLines(sw.GetStringBuilder().ToString()).Value;
            // kja to implement
            throw new NotImplementedException();
        }
    }
}