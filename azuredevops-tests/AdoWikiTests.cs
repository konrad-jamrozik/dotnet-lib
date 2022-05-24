using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;

namespace Wikitools.AzureDevOps.Tests;

[TestFixture]
public class AdoWikiTests
{
    // kja CURR WORK
    [Test]
    [Ignore("WIP")]
    public async Task Test()
    {
        // TO-DO: populate with proper values
        string wikiUriStr = "foo";
        string patEnvVar = "bar";
        IEnvironment env = new Environment();
        DateDay today = SimulatedTimeline.UtcNowDay;
        int pageId = 1;
        // TO-DO use builder pattern to create stats with a page with pageId.
        var expected = ValidWikiPagesStatsTestDataFixture.PageStats.AllPagesStats;
        var simulatedWikiHttpClient = new SimulatedWikiHttpClient(expected);
        
        // TO-DO need to modify AdoWiki to make the simulated client injectable.
        var adoWiki = new AdoWiki(wikiUriStr, patEnvVar, env, today);

        // Act
        var actual = await adoWiki.PageStats(PageViewsForDays.ForLastDays(1), pageId);

        new JsonDiffAssertion(expected, actual).Assert();

    }
}