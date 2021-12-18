using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Tests;

// kja curr work
public class GitFileStatsTests
{
    [Fact]
    public void SumsByFilePathWithRenamePresent()
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
                    new GitLogCommit.Numstat(0, 0, "/abc/def/{foo.md => bar.md}"),
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(6, 2, "/abc/def/bar.md")
                }),
            });

        Assert.Single(stats);
        Assert.Equal("/abc/def/bar.md", stats[0].FilePath);
        Assert.Equal(36, stats[0].Insertions);
        Assert.Equal(12, stats[0].Deletions);
    }
}