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
        var pagesStatsData   = data.WikiPagesStats;
        var pageViewsForDays = 30;

        // Arrange simulations
        var timeline = new SimulatedTimeline();
        var wiki     = new SimulatedAdoWiki(ValidWikiPagesStatsFixture.Build(pagesStatsData));

        // Arrange SUT declaration
        var pagesStats = wiki.PagesStats(pageViewsForDays);
        var sut        = new PageViewStatsReport(timeline, wiki, pageViewsForDays);

        var expected = new MarkdownDocument(Task.FromResult(new object[]
        {
            string.Format(PageViewStatsReport.DescriptionFormat,
                pageViewsForDays,
                timeline.UtcNow,
                pagesStatsData.Length) + MarkdownDocument.LineBreakMarker,
            "" + MarkdownDocument.LineBreakMarker,
            new TabularData(
                HeaderRow: PageViewStats.HeaderRow,
                Rows: data.ExpectedRows[(nameof(PageViewStatsReportTests), pagesStatsData)])
        }));

        // Act
        await new MarkdownDocumentDiff(expected, sut).Verify();
    }
}