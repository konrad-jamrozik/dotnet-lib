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
        //  - this includes weaving in wiki data stats
        // - simplify fixture logic
        [Fact]
        public async Task WritesWikiTableOfContents()
        {
            // var fs = new SimulatedFileSystem();
            // fs.FileTree()
            var pathsTrie = new FilePathTrie(new List<string> { "foo\\bar", "foo\\qux" });
            var validWikiPagesStats = ValidWikiPagesStatsFixture.Build(
                new[] { new WikiPageStats("foo\\bar", 1, Array.Empty<WikiPageStats.DayStat>()) }) 
                as IEnumerable<WikiPageStats>;

            var toc = new WikiTableOfContents(
                Task.FromResult(pathsTrie), 
                Task.FromResult(validWikiPagesStats));

            // Arrange expectations
            var expected = new MarkdownDocument(Task.FromResult(new object[]
            {
                "foo",
                "foo/bar",
                "foo/qux"
            }));

            await new MarkdownDocumentDiff(expected, toc).Verify();
        }
    }
}