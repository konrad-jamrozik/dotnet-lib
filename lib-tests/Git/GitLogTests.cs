using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Lib.Tests.Git;

public class GitLogTests
{
    [Fact]
    public async void GetsCommits()
    {
        var timeline = new SimulatedTimeline();
        int daysAgo = 5;
        var daySpan = daysAgo.AsDaySpanUntil(timeline.UtcNowDay.AddDays(-1));
        var os = new SimulatedOS(GitLogProcess(timeline, daySpan, daysAgo));
        var fs = new SimulatedFileSystem();
        var gitRepoDir = fs.NextSimulatedDir();
        var gitExecutablePath = "unused";

        var decl = new GitLogDeclare();
        var gitLog = decl.GitLog(timeline, os, gitRepoDir, gitExecutablePath);

        // Act
        GitLogCommits commits = await gitLog.Commits(daysAgo);
            
        var firstCommit = commits.First();
        var lastCommit = commits.Last();

        Assert.Equal(2, commits.Count());
        Assert.Equal(0, DateDay.Compare(firstCommit.Date, daySpan.StartDay));
        Assert.Equal(0, DateDay.Compare(lastCommit.Date, timeline.UtcNowDay.AddDays(-1)));
    }

    private static SimulatedGitLogProcess GitLogProcess(
        SimulatedTimeline timeline,
        DaySpan daySpan,
        int daysAgo)
    {
        var gitLogProcess = new SimulatedGitLogProcess(
            daySpan,
            new GitLogCommit[]
            {
                new GitLogCommit(
                    "PreFooAuthor",
                    timeline.UtcNow.AddDays(-daysAgo-1), // Should be excluded from the returned commits
                    new GitLogCommit.Numstat[] { new GitLogCommit.Numstat(1, 1, "PreFooPath") }),
                new GitLogCommit(
                    "FooAuthor",
                    timeline.UtcNow.AddDays(-daysAgo), // Should be included in the returned commits
                    new GitLogCommit.Numstat[] { new GitLogCommit.Numstat(2, 2, "FooPath") }),
                new GitLogCommit(
                    "BarAuthor",
                    timeline.UtcNow.AddDays(-1), // Should be included in the returned commits
                    new GitLogCommit.Numstat[] { new GitLogCommit.Numstat(3, 3, "BarPath") }),
                new GitLogCommit(
                    "PostBarAuthor",
                    timeline.UtcNow, // Should be excluded from the returned commits
                    new GitLogCommit.Numstat[] { new GitLogCommit.Numstat(4, 4, "PostBarPath") })
            });
        return gitLogProcess;
    }
}