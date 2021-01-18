using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Markdown.Tests;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;
using Xunit;
using static Wikitools.Declare;

namespace Wikitools.Tests
{
    public class GitAuthorsStatsReportTests
    {
        [Fact]
        public async Task ReportsGitAuthorsStats()
        {
            // Arrange inputs
            var data              = new Data();
            var commitsData       = data.CommitsLogs;
            var logDays           = 15;
            var gitExecutablePath = @"C:\Program Files\Git\bin\sh.exe";
            var gitRepoDirPath    = @"C:\Users\fooUser\barRepo";
            var top               = 5;

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var os       = new SimulatedOS(new SimulatedGitLogProcess(logDays, commitsData));

            // Arrange SUT declaration
            var gitLog  = GitLog(os, gitRepoDirPath, gitExecutablePath);
            var commits = gitLog.Commits(logDays);
            var sut     = new GitAuthorsStatsReport(timeline, logDays, commits, top);

            // Arrange expectations
            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                string.Format(GitAuthorsStatsReport.DescriptionFormat, logDays, timeline.UtcNow),
                "",
                new TabularData((GitAuthorsStatsReport.HeaderRow,
                    data.ExpectedRows[(nameof(GitAuthorsStatsReportTests), commitsData)]))
            }));

            await new MarkdownDocumentDiff(expected, sut).Verify();
        }
    }
}