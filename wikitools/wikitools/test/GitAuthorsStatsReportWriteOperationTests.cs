using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;
using Xunit;

namespace Wikitools.Tests
{
    public class GitAuthorsStatsReportWriteOperationTests
    {
        [Fact]
        public async Task GitAuthorsStatsReportWriteOperationSucceeds()
        {
            // Arrange inputs
            var changesStats = Data.ChangesStats;
            var logDays = 15;
            var gitExecutablePath = @"C:\Program Files\Git\bin\sh.exe";
            var gitRepoDirPath = @"C:\Users\fooUser\barRepo";

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var os = new SimulatedOS(new SimulatedGitLog(changesStats, logDays));

            // Arrange SUT declaration
            var sut = new GitAuthorsStatsReportWriteOperation(
                timeline,
                os,
                gitRepoDirPath,
                gitExecutablePath,
                logDays);

            // Arrange expectations
            var expected = new TabularData(
                Description: string.Format(GitAuthorsStatsReport.DescriptionFormat, logDays, timeline.UtcNow),
                HeaderRow: GitAuthorsStatsReport.HeaderRowLabels,
                Rows: (List<List<object>>) Data.Expectation[changesStats]);

            await Verify(sut, expected);
        }

        private static async Task Verify(GitAuthorsStatsReportWriteOperation sut, TabularData expected) =>
            AssertNoDiffBetween(expected, await Act(sut));

        private static async Task<TabularData> Act(GitAuthorsStatsReportWriteOperation sut)
        {
            // Arrange output sink
            await using var sw = new StringWriter();

            // Act
            await sut.ExecuteAsync(sw);

            return (TabularData) new MarkdownTable(sw).Data;
        }

        private static void AssertNoDiffBetween(TabularData expected, TabularData actual)
        {
            var jsonDiff = new JsonDiff(expected, actual);
            Assert.True(jsonDiff.IsEmpty, $"The expected baseline is different than actual target. Diff:\r\n{jsonDiff}");
        }
    }
}