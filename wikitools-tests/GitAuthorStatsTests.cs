using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Tests;

public class GitAuthorStatsTests
{
    [Fact]
    public void SumsByAuthor()
    {
        var now = new SimulatedTimeline().UtcNow;
        var stats = GitAuthorStats.SumByAuthor(
            new[]
            {
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(30, 10, "foo.txt")
                }),
                new GitLogCommit("Author2", now, new[]
                {
                    new GitLogCommit.Numstat(30, 10, "foo.txt"),
                    new GitLogCommit.Numstat(30, 10, "bar.txt")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(30, 10, "foo.txt")
                }),
                new GitLogCommit("Author2", now, new[]
                {
                    new GitLogCommit.Numstat(30, 10, "foo.txt"),
                    new GitLogCommit.Numstat(30, 10, "qux.txt")
                })
            });

        Assert.Equal(2, stats.Length);
        Assert.Equal(1, stats[0].FilesChanges);
        Assert.Equal(3, stats[1].FilesChanges);
    }

    [Fact]
    public void SumsByAuthorRenamedFile()
    {
        var now = new SimulatedTimeline().UtcNow;
        var stats = GitAuthorStats.SumByAuthor(
            new[]
            {
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(100, 6, "abc/def/foo.txt")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(200, 5, "abc/{def => ghi}/foo.txt")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(300, 4, "abc/ghi/{foo.txt => bar.txt}")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(400, 3, "abc/ghi/bar.txt")
                })
            }.Reverse());

        Assert.Single(stats);
        Assert.Equal(1, stats[0].FilesChanges);
        Assert.Equal(100+200+300+400, stats[0].Insertions);
        Assert.Equal(6+5+4+3, stats[0].Deletions);
    }

}