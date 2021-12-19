using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Tests;

// kja curr test
public class GitFileStatsTests
{
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
                    new GitLogCommit.Numstat(0, 0, "/abc/def/{foo.md => bar.md}"),
                }),
                // kja interleave in this tests also other rename chain
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(6, 2, "/abc/def/bar.md")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(0, 0, "/abc/def/{bar.md => qux.md}")
                }),
                new GitLogCommit("Author1", now, new[]
                {
                    new GitLogCommit.Numstat(100, 400, "/abc/def/qux.md")
                }),
            });

        Assert.Single(stats);
        Assert.Equal("/abc/def/qux.md", stats[0].FilePath);
        Assert.Equal(136, stats[0].Insertions);
        Assert.Equal(412, stats[0].Deletions);
    }
}