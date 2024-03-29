using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Tests;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Data;
using Wikitools.Lib.Tests.Markdown;
using Xunit;

namespace Wikitools.Tests;

public class PageViewStatsReportTests
{
    [Fact]
    public async Task ReportsPageViewStats()
    {
        // Arrange inputs
        var data             = new ReportTestsData();
        var pagesStatsData   = data.WikiPagesStats(daysOffset: -1); 
        var daysAgo          = 29;

        // Arrange simulations
        var timeline = new SimulatedTimeline();
        var today = timeline.UtcNowDay;
        var wikiClient =
            new SimulatedWikiHttpClient(ValidWikiPagesStatsFixture.Build(pagesStatsData), today);
        var wiki = new AdoWiki(wikiClient);

        // Arrange SUT declaration
        var sut = new PageViewStatsReport(timeline, wiki, daysAgo);

        var expected = new MarkdownDocument(Task.FromResult(new object[]
        {
            string.Format(PageViewStatsReport.ReportHeaderFormatString,
                daysAgo.AsDaySpanUntil(timeline.UtcNowDay.AddDays(-1)).ToPrettyString(),
                timeline.UtcNow,
                pagesStatsData.Length) + MarkdownDocument.LineBreakMarker,
            "" + MarkdownDocument.LineBreakMarker,
            new TabularData(
                HeaderRow: PageViewStats.HeaderRow,
                Rows: data.ExpectedRows[nameof(PageViewStatsReportTests)])
        }));

        // Act
        await new MarkdownDocumentDiff(expected, sut).Verify();
    }
}