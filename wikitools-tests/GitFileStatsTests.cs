using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Tests;

// kja curr tests
public class GitFileStatsTests
{
    [Fact]
    public void SumsByFilePathsWithSimpleRename()
    {
        var now = new SimulatedTimeline().UtcNow;
        var stats = GitFileStats.SumByFilePath(
            new[]
            {
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(30, 10, "/abc/def/foo.md")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(3, 1, "/abc/def/{foo.md => bar.md}")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(6, 2, "/abc/def/bar.md")
                }),
            });

        Assert.Single(stats);
        Assert.Equal("/abc/def/bar.md", stats[0].FilePath);
        Assert.Equal(30 + 3 + 6, stats[0].Insertions);
        Assert.Equal(10 + 1 + 2, stats[0].Deletions);
    }

    [Fact]
    public void SumsByFilePathsWithRenamePresent()
    {
        var now = new SimulatedTimeline().UtcNow;
        var stats = GitFileStats.SumByFilePath(
            new[]
            {
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(30, 10, "/abc/def/foo.md")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(25, 15, "/abc/def/{foo.md => bar.md}"),
                }),
                // kja interleave in this tests also other rename chain
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(6, 2, "/abc/def/bar.md")
                }),
                // kja interleave rename loop
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(10, 1, "/abc/def/{bar.md => qux.md}")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(100, 400, "/abc/def/qux.md")
                }),
            });

        Assert.Single(stats);
        Assert.Equal("/abc/def/qux.md", stats[0].FilePath);
        Assert.Equal(30 + 25 + 6 + 10 + 100, stats[0].Insertions);
        Assert.Equal(10 + 15 + 2 + 1 + 400, stats[0].Deletions);

    }
}