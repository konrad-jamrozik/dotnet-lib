using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Lib.Tests.Git
{
    public class GitLogTests
    {
        // kja currently failing test
        // For context, see todo in Wikitools.Lib.Git.GitLog.Commits
        [Fact]
        public async void TestGitLog()
        {
            var timeline = new SimulatedTimeline();
            int dayRange = 5;
            var daysAgo = timeline.UtcNow.AddDays(-5);
            var os = new SimulatedOS(Array.Empty<IProcessSimulationSpec>());

            var gitBashShell = new GitBashShell(
                os,
                "GitExecutablePath");
            
            var gitLog = new GitLog(
                timeline,
                new GitRepository(
                    gitBashShell,
                    new Dir(new SimulatedFileSystem(), "Path")));

            // Act
            GitLogCommits commits = await gitLog.Commits(dayRange);
            
            var firstCommit = commits.First();
            var lastCommit = commits.Last();

            Assert.True(firstCommit.Date.CompareTo(daysAgo) < 0);
            Assert.True(lastCommit.Date.CompareTo(timeline.UtcNow) > 0);
        }
    }
}
