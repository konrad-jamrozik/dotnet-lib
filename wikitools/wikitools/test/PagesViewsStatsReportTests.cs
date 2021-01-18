using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;
using Xunit;
using static Wikitools.Declare;
using static Wikitools.Lib.Tests.Tables.TabularDataAssertionExtensions;

namespace Wikitools.Tests
{
    public class PagesViewsStatsReportTests
    {
        [Fact]
        public async Task ReportsPagesViewsStats()
        {
            // Arrange inputs
            var data             = new Data();
            var pageStats        = data.PageStats;
            var wikiUri          = "https://dev.azure.com/adoOrg/adoProject/_wiki/wikis/wikiName";
            var patEnvVar        = string.Empty;
            var pageViewsForDays = 30;

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var adoApi   = new SimulatedAdoApi(pageStats);

            // Arrange SUT declaration
            var wiki       = Wiki(adoApi, wikiUri, patEnvVar, pageViewsForDays);
            // kja this is wrong: this wait shouldn't be necessary. Defer!
            var pagesStats = await wiki.PagesStats();
            var sut        = new PagesViewsStatsReport2(timeline, pageViewsForDays, pagesStats);

            var expected = new MarkdownDocument(new object[]
            {
                string.Format(PagesViewsStatsReport2.DescriptionFormat,
                    pageViewsForDays,
                    timeline.UtcNow,
                    pagesStats.Length),
                "",
                new TabularData2(
                    HeaderRow: PagesViewsStatsReport2.HeaderRow,
                    Rows: data.ExpectedRows[(nameof(PagesViewsStatsReportTests), pageStats)])
            });

            await Verify(expected, sut);
        }
    }
}