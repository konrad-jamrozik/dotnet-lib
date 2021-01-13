using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;
using Xunit;

namespace Wikitools.Tests
{
    public class PageViewsStatsReportTests
    {
        [Fact]
        public async Task Reports()
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
            var wiki = Declare.Wiki(adoApi, wikiUri, patEnvVar, pageViewsForDays);
            var sut  = new PageViewsStatsReport(timeline, wiki, pageViewsForDays);

            // Arrange expectations
            var expected = new TabularData(
                Description: string.Format(PageViewsStatsReport.DescriptionFormat,
                    pageViewsForDays,
                    timeline.UtcNow,
                    pageStats.Count),
                HeaderRow: PageViewsStatsReport.HeaderRowLabels,
                Rows: (List<List<object>>) Data.Expectation[pageStats]);

            await Verify(sut, expected);
        }

        // kja deduplicate with the other op test; add interface for op
        private static async Task Verify(ITabularData sut, TabularData expected) =>
            AssertNoDiffBetween(expected, await Act(sut));

        private static async Task<TabularData> Act(ITabularData sut)
        {
            // Arrange output sink
            await using var sw = new StringWriter();

            // Act
            await new MarkdownTable(sut).WriteAsync(sw);

            return (TabularData) new MarkdownTable(sw).Data;
        }

        private static void AssertNoDiffBetween(TabularData expected, TabularData actual)
        {
            var jsonDiff = new JsonDiff(expected, actual);
            Assert.True(jsonDiff.IsEmpty,
                $"The expected baseline is different than actual target. Diff:\r\n{jsonDiff}");
        }
    }
}