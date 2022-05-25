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
        var pageViewsForDays = new PageViewsForDays(3);
        var expected = input.Trim(pageViewsForDays);

        var adoWiki = new AdoWiki(new SimulatedWikiHttpClient(input), SimulatedTimeline.UtcNowDay);

        // Act
        var actual = await adoWiki.PagesStats(pageViewsForDays);

        new JsonDiffAssertion(expected, actual).Assert();
    }

    [Test]
    public async Task GetsPageStatsForPage()
    {
        int pageId = 1;
        var input = new ValidWikiPagesStatsFixture().WikiPagesStats();
        var pageViewsForDays = new PageViewsForDays(3);
        var expected = input.Trim(pageViewsForDays).Trim(pageId);

        var adoWiki = new AdoWiki(new SimulatedWikiHttpClient(input), SimulatedTimeline.UtcNowDay);

        // Act
        var actual = await adoWiki.PageStats(pageViewsForDays, pageId);

        new JsonDiffAssertion(expected, actual).Assert();
    }
}