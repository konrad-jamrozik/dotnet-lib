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
                    new GitLogCommit.Numstat(30, 10, "/foo.txt")
                }),
                new GitLogCommit("Author2", now, new[]
                {
                    new GitLogCommit.Numstat(30, 10, "/foo.txt"),
                    new GitLogCommit.Numstat(30, 10, "/bar.txt")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(30, 10, "/foo.txt")
                }),
                new GitLogCommit("Author2", now, new[]
                {
                    new GitLogCommit.Numstat(30, 10, "/foo.txt"),
                    new GitLogCommit.Numstat(30, 10, "/qux.txt")
                }),
            });

        Assert.Equal(2, stats.Length);
        Assert.Equal(1, stats[0].FilesChanges);
        Assert.Equal(3, stats[1].FilesChanges);
    }
}