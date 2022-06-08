using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;

namespace Wikitools.AzureDevOps.Tests;

[TestFixture]
public class AdoWikiTests
{
    [Test]
    public async Task GetsPagesStats()
    {
        var input = new ValidWikiPagesStatsFixture().WikiPagesStats();
        var days = 3;
        var expected = input.Trim(days);
        var today = new SimulatedTimeline().UtcNowDay;
        var adoWiki = new AdoWiki(new SimulatedWikiHttpClient(input, today));

        // Act
        var actual = await adoWiki.PagesStats(days);

        new JsonDiffAssertion(expected, actual).Assert();
    }

    [Test]
    public async Task GetsPageStatsForPage()
    {
        int pageId = 1;
        var input = new ValidWikiPagesStatsFixture().WikiPagesStats();
        var days = 3;
        var expected = input.Trim(days).TrimToPageId(pageId);
        var today = new SimulatedTimeline().UtcNowDay;
        var adoWiki = new AdoWiki(new SimulatedWikiHttpClient(input, today));

        // Act
        var actual = await adoWiki.PageStats(days, pageId);

        new JsonDiffAssertion(expected, actual).Assert();
    }
}