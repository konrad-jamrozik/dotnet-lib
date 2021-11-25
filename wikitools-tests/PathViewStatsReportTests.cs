using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Tests;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Data;
using Wikitools.Lib.Tests.Markdown;
using Xunit;

namespace Wikitools.Tests
{
    public class PathViewStatsReportTests
    {
        [Fact]
        public async Task ReportsPathViewStats()
        {
            // Arrange inputs
            var data             = new ReportTestsData();
            var pagesStatsData   = data.WikiPagesStats;
            var pageViewsForDays = 30;

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var adoWiki  = new SimulatedAdoWiki(ValidWikiPagesStatsFixture.Build(pagesStatsData));

            // Arrange SUT declaration
            var pagesStats = adoWiki.PagesStats(pageViewsForDays);
            var pathViewStats = PathViewStats.From(pagesStats.Result); // kj2 .Result
            var sut        = new PathViewStatsReport(timeline, pageViewsForDays, pathViewStats);

            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                string.Format(PathViewStatsReport.DescriptionFormat,
                    pageViewsForDays,
                    timeline.UtcNow,
                    pagesStatsData.Length),
                "",
                new TabularData(
                    HeaderRow: PathViewStats.HeaderRow,
                    Rows: data.ExpectedRows[(nameof(PathViewStatsReportTests), pagesStatsData)])
            }));

            // Act
            await new MarkdownDocumentDiff(expected, sut).Verify();
        }
    }
}