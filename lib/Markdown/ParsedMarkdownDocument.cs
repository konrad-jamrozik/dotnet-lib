using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;
using static MoreLinq.MoreEnumerable;

namespace Wikitools.Lib.Markdown;

public record ParsedMarkdownDocument : MarkdownDocument
{
    public ParsedMarkdownDocument(StringWriter sw) : base(Task.FromResult(GetContent(sw))) { }

    private static object[] GetContent(StringWriter sw)
    {
        string[] markdownLines = new TextLines(sw.GetStringBuilder().ToString()).SplitTrimmingEnd;

        var groups = markdownLines.GroupAdjacent(line => line.FirstOrDefault() == '|');

        var content = groups.SelectMany(group => 
                @group.Key // true if the group's key is "|" i.e. it is a table
                    ? Return(new MarkdownTable(@group.ToArray()).Data).Cast<object>() 
                    : @group.ToArray())
            .ToArray();

        return content;
    }
}