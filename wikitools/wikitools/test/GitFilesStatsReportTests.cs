using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;
using Xunit;
using static Wikitools.Declare;

namespace Wikitools.Tests
{
    public class GitFilesStatsReportTests
    {
        [Fact]
        public async Task ReportsFilesChangesStats()
        {
            // Arrange inputs
            var changesStats      = Data.FileChangesStats;
            var logDays           = 15;
            var gitExecutablePath = @"C:\Program Files\Git\bin\sh.exe";
            var gitRepoDirPath    = @"C:\Users\fooUser\barRepo";

            // Arrange simulations
            var timeline      = new SimulatedTimeline();
            var gitLogCommits = new GitLogCommit[1]; // kja fix when ready
            var os            = new SimulatedOS(new SimulatedGitLogProcess(logDays, gitLogCommits)); 

            // Arrange SUT declaration
            var gitLog = GitLog(os, gitRepoDirPath, gitExecutablePath, logDays);
            // kja to replace with Report2
            // var sut    = new GitFilesStatsReport(timeline, gitLog, logDays);

            // Arrange expectations
            // var expected = new TabularData(
            //     Description: string.Format(GitFilesStatsReport.DescriptionFormat, logDays, timeline.UtcNow),
            //     HeaderRow: GitFilesStatsReport.HeaderRowLabels,
            //     Rows: (List<List<object>>) Data.Expectation[changesStats]);

            // kja temp turned off
            // await Verify(expected, sut);
        }
    }
}