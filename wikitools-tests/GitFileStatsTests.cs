using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Tests;

public class GitFileStatsTests
{
    [Fact]
    public void SumsByFilePathsWithSimpleRename()
    {
        var data = new[]
        {
            (30, 10, "abc/def/foo.md"),
            (3, 1, "abc/def/{foo.md => bar.md}"),
            (6, 2, "abc/def/bar.md")
        };
        var commits = GitLogCommits(data);

        // Act
        var stats = GitFileStats.SumByFilePath(commits);

        Assert.Single(stats);
        Assert.Equal("abc/def/bar.md", stats[0].FilePath);
        Assert.Equal(30 + 3 + 6, stats[0].Insertions);
        Assert.Equal(10 + 1 + 2, stats[0].Deletions);
    }

    [Fact]
    public void SumsByFilePathsWithRenamePresent()
    {
        var data = new[]
        {
            (100, 8, "abc/def/foo.md"),
            (200, 7, "{ => intro}/abc/def/foo.md"),
            (300, 6, "{intro/abc => }/def/foo.md"),
            (400, 5, "{ => abc}/def/foo.md"),
            (500, 4, "abc/{ => ghi/jkl}/def/foo.md"),
            (600, 3, "abc/ghi/jkl/def/foo.md"),
            (700, 2, "abc/ghi/jkl/def/{foo.md => qux.md}"),
            (800, 1, "abc/ghi/jkl/def/qux.md")
        };
        var commits = GitLogCommits(data);

        // Act
        var stats = GitFileStats.SumByFilePath(commits);

        Assert.Single(stats);
        Assert.Equal("abc/ghi/jkl/def/qux.md", stats[0].FilePath);
        Assert.Equal(100 + 200 + 300 + 400 + 500 + 600 + 700 + 800, stats[0].Insertions);
        Assert.Equal(8 + 7 + 6 + 5 + 4 + 3 + 2 + 1, stats[0].Deletions);
    }

    private static IEnumerable<GitLogCommit> GitLogCommits((int, int, string)[] inputData)
    {
        var now = new SimulatedTimeline().UtcNow;
        var data = inputData.Reverse().Select(
            t => new GitLogCommit(
                "Author1",
                now,
                new[] { new GitLogCommit.Numstat(t.Item1, t.Item2, t.Item3) }));
        return data;
    }
}