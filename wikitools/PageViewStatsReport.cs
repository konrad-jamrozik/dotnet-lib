using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record PageViewStatsReport : MarkdownDocument
{
    public const string DescriptionFormat =
        "Page views since last {0} days as of {1}. Total wiki pages: {2}";

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
        var pageViewStats = await PageViewStats.From(
            timeline,
            wiki,
            new DaySpan(timeline.UtcNow, daysAgo));
        return new object[]
        {
            string.Format(
                DescriptionFormat,
                daysAgo,
                timeline.UtcNow,
                pageViewStats.Count()),
            "",
            PageViewStats.TabularData(pageViewStats)
        };
    }
}