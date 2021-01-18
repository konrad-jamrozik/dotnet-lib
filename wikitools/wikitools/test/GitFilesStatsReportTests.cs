using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;
using Xunit;
using static Wikitools.Declare;
using static Wikitools.Lib.Tests.Tables.TabularDataAssertionExtensions;

namespace Wikitools.Tests
{
    public class GitFilesStatsReportTests
    {
        [Fact]
        public async Task ReportsFilesChangesStats()
        {
            // Arrange inputs
            var data              = new Data();
            var commitsData       = data.CommitsLogs;
            var logDays           = 15;
            var gitExecutablePath = @"C:\Program Files\Git\bin\sh.exe";
            var gitRepoDirPath    = @"C:\Users\fooUser\barRepo";

            // Arrange simulations
            var timeline = new SimulatedTimeline();
            var os       = new SimulatedOS(new SimulatedGitLogProcess(logDays, commitsData));

            // Arrange SUT declaration
            var gitLog = GitLog(os, gitRepoDirPath, gitExecutablePath, logDays);
            // kja this is wrong: this wait shouldn't be necessary. Defer!
            var commits = await gitLog.Commits(logDays);
            var sut     = new GitFilesStatsReport2(timeline, logDays, commits);

            // Arrange expectations
            var expected = new MarkdownDocument(new object[]
            {
                string.Format(GitFilesStatsReport2.DescriptionFormat, logDays, timeline.UtcNow),
                "",
                new TabularData2((GitFilesStatsReport2.HeaderRow, data.ExpectedRows[(nameof(GitFilesStatsReportTests), commitsData)]))
            });

            await Verify(expected, sut);
        }
    }
}