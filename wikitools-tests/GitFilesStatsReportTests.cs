using System.Threading.Tasks;
using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Data;
using Wikitools.Lib.Tests.Markdown;
using Xunit;

namespace Wikitools.Tests
{
    public class GitFilesStatsReportTests
    {
        [Fact]
        public async Task ReportsFilesChangesStats()
        {
            // Arrange inputs
            var data              = new ReportTestsData();
            var commitsData       = data.CommitsLogs;
            var logDays           = 15;
            var fs                = new SimulatedFileSystem();
            var gitRepoDir        = fs.NextSimulatedDir();
            var gitExecutablePath = "unused";
            var top               = 5;

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var os = new SimulatedOS(new SimulatedGitLogProcess(timeline, logDays, commitsData));

            // Arrange SUT declaration
            var gitLog  = new GitLogDeclare().GitLog(timeline, os, gitRepoDir, gitExecutablePath);
            var commits = gitLog.Commits(logDays);
            var stats   = GitFileStats.From(commits.Result, top: top); // kj2 .Result
            var sut     = new GitFilesStatsReport(timeline, logDays, stats);

            // Arrange expectations
            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                string.Format(GitFilesStatsReport.DescriptionFormat, logDays, timeline.UtcNow)
                + MarkdownDocument.LineBreakMarker,
                "" + MarkdownDocument.LineBreakMarker,
                new TabularData((GitFileStats.HeaderRow,
                    data.ExpectedRows[(nameof(GitFilesStatsReportTests), commitsData)]))
            }));

            await new MarkdownDocumentDiff(expected, sut).Verify();
        }
    }
}