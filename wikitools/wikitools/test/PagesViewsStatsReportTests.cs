using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Data;
using Wikitools.Lib.Tests.Markdown;
using Xunit;
using static Wikitools.Declare;

namespace Wikitools.Tests
{
    public class PagesViewsStatsReportTests
    {
        [Fact]
        public async Task ReportsPagesViewsStats()
        {
            // Arrange inputs
            var data             = new ReportTestsData();
            var pagesStatsData   = data.PagesStats;
            var pageViewsForDays = 30;

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var adoWiki  = new SimulatedAdoWiki(pagesStatsData);

            // Arrange SUT declaration
            var pagesStats = adoWiki.PagesStats(pageViewsForDays);
            var sut        = new PagesViewsStatsReport(timeline, pagesStats, pageViewsForDays);

            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                string.Format(PagesViewsStatsReport.DescriptionFormat,
                    pageViewsForDays,
                    timeline.UtcNow,
                    pagesStatsData.Length),
                "",
                new TabularData(
                    HeaderRow: PagesViewsStatsReport.HeaderRow,
                    Rows: data.ExpectedRows[(nameof(PagesViewsStatsReportTests), pagesStatsData)])
            }));

            await new MarkdownDocumentDiff(expected, sut).Verify();
        }
    }
}