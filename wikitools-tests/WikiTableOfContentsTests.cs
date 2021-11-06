using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Tests;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Tests.Markdown;
using Xunit;

namespace Wikitools.Tests
{
    public class WikiTableOfContentsTests
    {
        // kja 3 curr work:
        // - fix expectations to be on what I proposed in WikiTableOfContents comment
        //   - this includes weaving in wiki data stats
        // - simplify fixture logic
        // - there should be another test like that, but integration,
        // obtaining the input data from local clone of given wiki, per config.
        //   - For that, see Wikitools.Tests.Tools.WriteOutGitRepoClonePaths.
        [Fact]
        public async Task WritesWikiTableOfContents()
        {
            var wikiPagesPrefix = AdoWikiPagesPaths.WikiPagesPrefix;
            var gitCloneRootPaths =
                new List<string> { "foo.md", "foo\\bar.md" }.Select(
                    path => wikiPagesPrefix + path);
            var adoWikiPagesPaths = new AdoWikiPagesPaths(gitCloneRootPaths);

            var validWikiPagesStats =
                // kj2 this could be simplified to
                // ValidWikiPagesStatsFixture.Build(("foo.md", 10), ("bar\\bar.md",25));
                ValidWikiPagesStatsFixture.Build(
                        new WikiPageStats[]
                        {
                            new(
                                "/foo",
                                1,
                                new WikiPageStats.DayStat[] { new(10, ValidWikiPagesStatsFixture.Today) }),
                            new(
                                "/foo/bar",
                                2,
                                new WikiPageStats.DayStat[] { new(25, ValidWikiPagesStatsFixture.Today) })

                        }) 
                as IEnumerable<WikiPageStats>;

            var tocUT = new WikiTableOfContents(
                adoWikiPagesPaths,
                Task.FromResult(validWikiPagesStats));

            // Arrange expectations
            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                "/foo",
                "/foo/bar",
            }));

            // Act
            await new MarkdownDocumentDiff(expected, tocUT).Verify();
        }
    }
}