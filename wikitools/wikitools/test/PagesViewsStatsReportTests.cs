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
            var wikiUri          = "https://dev.azure.com/adoOrg/adoProject/_wiki/wikis/wikiName";
            var patEnvVar        = string.Empty;
            var pageViewsForDays = 30;

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var adoApi   = new SimulatedAdoApi(pagesStatsData);

            // Arrange SUT declaration
            var wiki       = Wiki(adoApi, wikiUri, patEnvVar);
            var pagesStats = wiki.PagesStats(pageViewsForDays);
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