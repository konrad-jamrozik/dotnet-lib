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
        var commitsData       = data.CommitsLogs;
        var timeline          = new SimulatedTimeline();
        var commitDays        = 15;
        var logDaysSpan       = new DaySpan(timeline.UtcNow, commitDays);
        var fs                = new SimulatedFileSystem();
        var gitRepoDir        = fs.NextSimulatedDir();
        var gitExecutablePath = "unused";
        var top               = 5;
        var os = new SimulatedOS(new SimulatedGitLogProcess(timeline, logDaysSpan, commitsData));

        // Arrange SUT declaration
        var gitLog = new GitLogDeclare().GitLog(timeline, os, gitRepoDir, gitExecutablePath);
        var stats  = GitFileStats.From(gitLog, commitDays, top: top);
        var sut    = new GitFilesStatsReport(timeline, commitDays, stats);

        // Arrange expectations
        var expected = new MarkdownDocument(Task.FromResult(new object[]
        {
            string.Format(GitFilesStatsReport.DescriptionFormat, commitDays, timeline.UtcNow)
            + MarkdownDocument.LineBreakMarker,
            "" + MarkdownDocument.LineBreakMarker,
            new TabularData((GitFileStats.HeaderRow,
                data.ExpectedRows[(nameof(GitFilesStatsReportTests), commitsData)]))
        }));

        await new MarkdownDocumentDiff(expected, sut).Verify();
    }
}