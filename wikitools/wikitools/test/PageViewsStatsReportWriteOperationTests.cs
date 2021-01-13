using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
using Wikitools.Lib.Json;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;
using Xunit;

namespace Wikitools.Tests
{
    public class PageViewsStatsReportWriteOperationTests
    {
        [Fact]
        public async Task PageViewsStatsReportWriteOperationSucceeds()
        {
            // Arrange inputs
            var    wikiStats               = string.Empty; // kja fill out and inject to simulated ado API. Also provide proper expectation.
            var    logDays                 = 15;
            string adoWikiUri              = "https://dev.azure.com/adoOrg/adoProject/_wiki/wikis/wikiName";
            string adoPatEnvVar            = "fakeEnvVarName";
            int    adoWikiPageViewsForDays = 30;
            var    wikiPagesCount          = 10;
            
            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var adoApi   = new SimulatedAdoApi();

            // Arrange SUT declaration
            var sut = new PageViewsStatsReportWriteOperation(
                timeline,
                adoApi,
                adoWikiUri,
                adoPatEnvVar,
                adoWikiPageViewsForDays);

            // Arrange expectations
            var expected = new TabularData(
                Description: string.Format(PageViewsStatsReport.DescriptionFormat, logDays, timeline.UtcNow, wikiPagesCount),
                HeaderRow: PageViewsStatsReport.HeaderRowLabels,
                Rows: Data.Expectation[wikiStats] as List<List<object>>);

            await Verify(sut, expected);
        }

        // kja deduplicate with the other op test; add interface for op
        private static async Task Verify(PageViewsStatsReportWriteOperation sut, TabularData expected) =>
            AssertNoDiffBetween(expected, await Act(sut));

        private static async Task<TabularData> Act(PageViewsStatsReportWriteOperation sut)
        {
            // Arrange output sink
            await using var sw = new StringWriter();

            // Act
            await sut.ExecuteAsync(sw);

            return new MarkdownTable(sw).Data as TabularData;
        }

        private static void AssertNoDiffBetween(TabularData expected, TabularData actual)
        {
            var jsonDiff = new JsonDiff(expected, actual);
            Assert.True(jsonDiff.IsEmpty, $"The expected baseline is different than actual target. Diff:\r\n{jsonDiff}");
        }
    }
}