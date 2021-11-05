using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Tests;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.OS;
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
            var gitCloneRootPaths = new List<string> { "foo\\bar", "foo\\qux", "foo\\baz\\bar" };
            var adoWikiPagesPaths = new AdoWikiPagesPaths(gitCloneRootPaths);

            var validWikiPagesStats =
                ValidWikiPagesStatsFixture.Build(
                        new WikiPageStats[]
                        {
                            new(
                                "foo\\bar",
                                1,
                                new WikiPageStats.DayStat[] { new(10, ValidWikiPagesStatsFixture.Today) })
                        }) 
                as IEnumerable<WikiPageStats>;

            var toc = new WikiTableOfContents(
                adoWikiPagesPaths,
                Task.FromResult(validWikiPagesStats));

            // Arrange expectations
            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                "foo",
                "foo/bar",
                "foo/qux",
                "foo/baz/bar"
            }));

            await new MarkdownDocumentDiff(expected, toc).Verify();
        }
    }
}