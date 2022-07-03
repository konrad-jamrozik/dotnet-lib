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

public class GitAuthorsStatsReportTests
{
    // kj2-tests this test is super hard to work with, refactor, and similar tests.
    // kj2-migration move to newest C#
    [Fact] // kj2-migration use everywhere NUnit
    public async Task ReportsGitAuthorsStats()
    {
        // Arrange inputs and simulations
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
        var sut = new GitAuthorsStatsReport(timeline, gitLog, top, commitDays);

        // Arrange expectations
        var expected = new MarkdownDocument(Task.FromResult(new object[]
        {
            string.Format(
                GitAuthorsStatsReport.ReportHeaderFormatString,
                commitDays,
                timeline.UtcNow) 
            + MarkdownDocument.LineBreakMarker,
            "" + MarkdownDocument.LineBreakMarker,
            new TabularData((GitAuthorStats.HeaderRow,
                data.ExpectedRows[nameof(GitAuthorsStatsReportTests)]))
        }));

        await new MarkdownDocumentDiff(expected, sut).Verify();
    }
}