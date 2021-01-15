using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Tests
{
    public class GitFilesStatsReportTests
    {
        [Fact]
        public async Task Reports()
        {
            var log    = new SimulatedGitLogProcess(new List<GitAuthorChangeStats>(), 1);
            var gitLog = Declare.GitLog(new SimulatedOS(log), "", "", 3);
            var report = new GitFilesStatsReport(new SimulatedTimeline(), gitLog, 1);

            // kja proof of concept of composable transformations
            report = report with { Rows = new AsyncLazy<List<List<object>>>(async () => (await report.Rows.Value).Take(3).ToList()) };
            report = report with { Rows = (await report.Rows.Value).AsyncLazy() };
            report = report with { Rows = report.Rows.AsyncLazy(r => r) };
        }
    }
}