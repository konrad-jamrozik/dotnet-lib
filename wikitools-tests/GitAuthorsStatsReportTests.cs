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
    public class GitAuthorsStatsReportTests
    {
        [Fact] // kj2 use everywhere NUnit 
        public async Task ReportsGitAuthorsStats()
        {
            // Arrange inputs and simulations
            var data              = new ReportTestsData();
            var commitsData       = data.CommitsLogs;
            var timeline          = new SimulatedTimeline();
            var logDays           = 15;
            var logDaysSpan        = new DaySpan(timeline.UtcNow, logDays);
            var fs                = new SimulatedFileSystem();
            var gitRepoDir        = fs.NextSimulatedDir();
            var gitExecutablePath = "unused";
            var top               = 5;
            var os = new SimulatedOS(new SimulatedGitLogProcess(timeline, logDaysSpan, commitsData));

            // Arrange SUT declaration
            var gitLog  = new GitLogDeclare().GitLog(timeline, os, gitRepoDir, gitExecutablePath);
            var commits = gitLog.Commits(logDays);
            // kj2 .Result
            var stats   = GitAuthorStats.From(commits.Result, top: top);
            var sut     = new GitAuthorsStatsReport(timeline, logDays, stats);

            // Arrange expectations
            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                string.Format(GitAuthorsStatsReport.DescriptionFormat, logDays, timeline.UtcNow) 
                + MarkdownDocument.LineBreakMarker,
                "" + MarkdownDocument.LineBreakMarker,
                new TabularData((GitAuthorStats.HeaderRow,
                    data.ExpectedRows[(nameof(GitAuthorsStatsReportTests), commitsData)]))
            }));

            await new MarkdownDocumentDiff(expected, sut).Verify();
        }
    }
}