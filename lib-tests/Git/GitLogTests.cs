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
        // kja clean up this test and also prove that 
        // the commits _out_ of range are not included.
        // For context, see todo in Wikitools.Lib.Git.GitLog.Commits
        [Fact]
        public async void TestGitLog()
        {
            var timeline = new SimulatedTimeline();
            int dayRange = 5;
            var daysAgo = timeline.UtcNow.AddDays(-5);
            var os = new SimulatedOS(GitLogProcess(timeline));
            var fs = new SimulatedFileSystem();
            var gitRepoDir        = fs.NextSimulatedDir();
            var gitExecutablePath = "unused";

            var decl = new GitLogDeclare2();
            var gitLog = decl.GitLog(timeline, os, gitRepoDir, gitExecutablePath);

            // Act
            GitLogCommits commits = await gitLog.Commits(dayRange);
            
            var firstCommit = commits.First();
            var lastCommit = commits.Last();

            Assert.True(firstCommit.Date.CompareTo(daysAgo) < 0);
            Assert.True(lastCommit.Date.CompareTo(new DateDay(timeline.UtcNow)) >= 0);
        }

        private static SimulatedGitLogProcess GitLogProcess(SimulatedTimeline timeline)
        {
            var gitLogProcess = new SimulatedGitLogProcess(
                timeline,
                AfterDays: 5,
                new GitLogCommit[]
                {
                    new GitLogCommit(
                        "FooAuthor",
                        timeline.UtcNow.AddDays(-5),
                        new GitLogCommit.Numstat[] { new GitLogCommit.Numstat(1, 2, "FooPath") }),
                    new GitLogCommit(
                        "BarAuthor",
                        timeline.UtcNow,
                        new GitLogCommit.Numstat[] { new GitLogCommit.Numstat(1, 2, "BarPath") })
                });
            return gitLogProcess;
        }
    }
}
