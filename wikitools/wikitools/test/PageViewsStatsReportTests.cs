using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;
using Xunit;
using static Wikitools.Declare;
using static Wikitools.Lib.Tests.Tables.TabularDataAssertionExtensions;

namespace Wikitools.Tests
{
    public class PageViewsStatsReportTests
    {
        [Fact]
        public async Task ReportsPagesViewsStats()
        {
            // Arrange inputs
            var pageStats        = Data.PageStats;
            var wikiUri          = "https://dev.azure.com/adoOrg/adoProject/_wiki/wikis/wikiName";
            var patEnvVar        = string.Empty;
            var pageViewsForDays = 30;

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var adoApi   = new SimulatedAdoApi(pageStats);

            // Arrange SUT declaration
            var wiki = Wiki(adoApi, wikiUri, patEnvVar, pageViewsForDays);
            var sut  = new PagesViewsStatsReport(timeline, wiki, pageViewsForDays);

            // Arrange expectations
            var expected = new TabularData(
                Description: string.Format(PagesViewsStatsReport.DescriptionFormat,
                    pageViewsForDays,
                    timeline.UtcNow,
                    pageStats.Length),
                HeaderRow: PagesViewsStatsReport.HeaderRowLabels,
                Rows: (List<List<object>>) Data.Expectation[pageStats]);

            await Verify(expected, sut);
        }
    }
}