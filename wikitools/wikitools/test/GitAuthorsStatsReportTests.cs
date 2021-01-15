using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;
using Xunit;
using static Wikitools.Declare;
using static Wikitools.Lib.Tests.Tables.TabularDataAssertionExtensions;

namespace Wikitools.Tests
{
    public class GitAuthorsStatsReportTests
    {
        [Fact]
        public async Task Reports()
        {
            // Arrange inputs
            var changesStats      = Data.AuthorChangesStats;
            var logDays           = 15;
            var gitExecutablePath = @"C:\Program Files\Git\bin\sh.exe";
            var gitRepoDirPath    = @"C:\Users\fooUser\barRepo";

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var os       = new SimulatedOS(new SimulatedGitLogProcess(changesStats, logDays));

            // Arrange SUT declaration
            var gitLog = GitLog(os, gitRepoDirPath, gitExecutablePath, logDays);
            var sut    = new GitAuthorsStatsReport(timeline, gitLog, logDays);

            // Arrange expectations
            var expected = new TabularData(
                Description: string.Format(GitAuthorsStatsReport.DescriptionFormat, logDays, timeline.UtcNow),
                HeaderRow: GitAuthorsStatsReport.HeaderRowLabels,
                Rows: (List<List<object>>) Data.Expectation[changesStats]);

            await Verify(expected, sut);
        }
    }
}