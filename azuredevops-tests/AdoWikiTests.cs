using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;

namespace Wikitools.AzureDevOps.Tests;

[TestFixture]
public class AdoWikiTests
{
    // kja CURR TEST
    [Test]
    [Ignore("WIP")]
    public async Task Test()
    {
        DateDay today = SimulatedTimeline.UtcNowDay;
        int pageId = 1;
        // TO-DO use builder pattern to create stats with a page with pageId.
        var expected = new ValidWikiPagesStatsFixture().WikiPagesStats();
        var simulatedWikiHttpClient = new SimulatedWikiHttpClient(expected);
        
        var adoWiki = new AdoWiki(simulatedWikiHttpClient, today);

        // Act
        var actual = await adoWiki.PageStats(PageViewsForDays.ForLastDays(1), pageId);

        new JsonDiffAssertion(expected, actual).Assert();

    }
}