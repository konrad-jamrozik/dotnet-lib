using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Tests;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Markdown;
using Xunit;

namespace Wikitools.Tests
{
    public class WikiTableOfContentsTests
    {
        [Fact]
        public async Task WritesWikiTableOfContents()
        {
            var timeline = new SimulatedTimeline();
            var wikiPagesPrefix = AdoWikiPagesPaths.WikiPagesPrefix;
            var gitCloneRootPaths =
                new List<string> { "foo.md", "foo\\bar.md" }.Select(
                    path => wikiPagesPrefix + path);
            var adoWikiPagesPaths = new AdoWikiPagesPaths(gitCloneRootPaths);

            var validWikiPagesStats =
                // kj2 WikiTableOfContentsTests fixture building / this could be simplified to
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
                        });

            var tocUT = new WikiTableOfContents(
                timeline,
                adoWikiPagesPaths,
                Task.FromResult(validWikiPagesStats));

            // Arrange expectations
            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                string.Format(WikiTableOfContents.PageHeader, timeline.UtcNow)
                + MarkdownDocument.LineBreakMarker,
                "" + MarkdownDocument.LineBreakMarker,
                "[/foo](/foo) - 10 views" + MarkdownDocument.LineBreakMarker ,
                "[/foo/bar](/foo/bar) - 25 views" + MarkdownDocument.LineBreakMarker
            }));

            // Act
            await new MarkdownDocumentDiff(expected, tocUT).Verify();
        }
    }
}