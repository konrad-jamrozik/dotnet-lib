using System.Linq;
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
        var stats = GitFileStats.SumByFilePath(new[]
        {
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(30, 10, "abc/def/foo.md")
            }),
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(3, 1, "abc/def/{foo.md => bar.md}")
            }),
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(6, 2, "abc/def/bar.md")
            }),
        }.Reverse());

        Assert.Single(stats);
        Assert.Equal("abc/def/bar.md", stats[0].FilePath);
        Assert.Equal(30 + 3 + 6, stats[0].Insertions);
        Assert.Equal(10 + 1 + 2, stats[0].Deletions);
    }

    // kja interleave in this tests also other rename chain
    [Fact] 
    public void SumsByFilePathsWithRenamePresent()
    {
        var now = new SimulatedTimeline().UtcNow;
        var stats = GitFileStats.SumByFilePath(new[]
        {
            // kja compress into tuples. Same for test above.
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(100, 8, "abc/def/foo.md")
            }),
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(200, 7, "{ => intro}/abc/def/foo.md")
            }),
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(300, 6, "{intro/abc => }/def/foo.md")
            }),
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(400, 5, "{ => abc}/def/foo.md")
            }),
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(500, 4, "abc/{ => ghi/jkl}/def/foo.md"),
            }),
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(600, 3, "abc/ghi/jkl/def/foo.md")
            }),
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(700, 2, "abc/ghi/jkl/def/{foo.md => qux.md}")
            }),
            new GitLogCommit("Author1", now, new[]
            {
                new GitLogCommit.Numstat(800, 1, "abc/ghi/jkl/def/qux.md")
            }),
        }.Reverse());

        Assert.Single(stats);
        Assert.Equal("abc/ghi/jkl/def/qux.md", stats[0].FilePath);
        Assert.Equal(100+200+300+400+500+600+700+800, stats[0].Insertions);
        Assert.Equal(8+7+6+5+4+3+2+1, stats[0].Deletions);

    }
}