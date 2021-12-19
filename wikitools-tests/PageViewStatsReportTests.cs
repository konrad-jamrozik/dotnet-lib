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
    // kja 2 test fails! fix!
    [Fact]
    public async Task ReportsPageViewStats()
    {
        // Arrange inputs
        var data             = new ReportTestsData();
        var pagesStatsData   = data.WikiPagesStats;
        var daysAgo          = 29;

        // Arrange simulations
        var timeline = new SimulatedTimeline();
        var wiki     = new SimulatedAdoWiki(ValidWikiPagesStatsFixture.Build(pagesStatsData));

        // Arrange SUT declaration
        var sut      = new PageViewStatsReport(timeline, wiki, daysAgo);

        var expected = new MarkdownDocument(Task.FromResult(new object[]
        {
            string.Format(PageViewStatsReport.DescriptionFormat,
                daysAgo,
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