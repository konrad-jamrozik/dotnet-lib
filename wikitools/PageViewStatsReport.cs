using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record PageViewStatsReport : MarkdownDocument
{
    public const string ReportHeaderFormatString =
        "Page views for {0} as of {1}. Total wiki pages: {2}";

    public PageViewStatsReport(
        ITimeline timeline,
        IAdoWiki wiki,
        int daysAgo) : base(
        GetContent(timeline, wiki, daysAgo)) { }

    private static async Task<object[]> GetContent(
        ITimeline timeline,
        IAdoWiki wiki,
        int daysAgo)
    {
        var daySpan = daysAgo.AsDaySpanUntil(new DateDay(timeline.UtcNow).Yesterday);

        var pageViewStats = await PageViewStats.From(timeline, wiki, daySpan);
        return new object[]
        {
            string.Format(
                ReportHeaderFormatString,
                daySpan.ToPrettyString(),
                timeline.UtcNow,
                pageViewStats.Count()),
            "",
            PageViewStats.TabularData(pageViewStats)
        };
    }
}