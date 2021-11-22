using System.Threading.Tasks;
using Wikitools.AzureDevOps;
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
            var os       = new SimulatedOS(new SimulatedGitLogProcess(logDays, commitsData));

            // Arrange SUT declaration
            var gitLog  = new GitLogDeclare().GitLog(os, gitRepoDir, gitExecutablePath);
            var commits = gitLog.Commits(logDays);
            var sut     = new GitFilesStatsReport(timeline, commits, logDays, top);

            // Arrange expectations
            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                string.Format(GitFilesStatsReport.DescriptionFormat, logDays, timeline.UtcNow),
                "",
                new TabularData((GitFilesStatsReport.HeaderRow,
                    data.ExpectedRows[(nameof(GitFilesStatsReportTests), commitsData)]))
            }));

            await new MarkdownDocumentDiff(expected, sut).Verify();
        }
    }
}