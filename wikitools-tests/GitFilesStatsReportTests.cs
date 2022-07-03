using System.Threading.Tasks;
using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Data;
using Wikitools.Lib.Tests.Markdown;
using Xunit;

namespace Wikitools.Tests;

public class GitFilesStatsReportTests
{
    [Fact]
    public async Task ReportsFilesChangesStats()
    {
        // Arrange inputs
        var data              = new ReportTestsData();
        var timeline          = new SimulatedTimeline();
        var lastDayOfCommits  = timeline.UtcNowDay.AddDays(-1);
        var commitsData       = data.GetCommitsLogs(lastDayOfCommits);
        var commitDays        = 15;
        var logDaysSpan       = new DaySpan(lastDayOfCommits, commitDays);
        var fs                = new SimulatedFileSystem();
        var gitRepoDir        = fs.NextSimulatedDir();
        var gitExecutablePath = "unused";
        var top               = 5;
        var os = new SimulatedOS(new SimulatedGitLogProcess(logDaysSpan, commitsData));

        // Arrange SUT declaration
        var gitLog = new GitLogDeclare().GitLog(timeline, os, gitRepoDir, gitExecutablePath);
        var stats  = await GitFileStats.From(gitLog, commitDays, top: top);
        var sut    = new GitFilesStatsReport(timeline, commitDays, stats);

        // Arrange expectations
        var expected = new MarkdownDocument(Task.FromResult(new object[]
        {
            string.Format(GitFilesStatsReport.ReportHeaderFormatString, commitDays, timeline.UtcNow)
            + MarkdownDocument.LineBreakMarker,
            "" + MarkdownDocument.LineBreakMarker,
            new TabularData((GitFileStats.HeaderRow,
                data.ExpectedRows[nameof(GitFilesStatsReportTests)]))
        }));

        await new MarkdownDocumentDiff(expected, sut).Verify();
    }
}