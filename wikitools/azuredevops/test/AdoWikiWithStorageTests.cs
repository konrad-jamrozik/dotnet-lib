using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;
using static Wikitools.Lib.Primitives.SimulatedTimeline;

namespace Wikitools.AzureDevOps.Tests
{
    [TestFixture]
    public class AdoWikiWithStorageTests
    {
        // kja test todos:
        // - DONE Everything empty: no data from wiki, no data from storage
        // - DONE Data in wiki, nothing in storage
        // - DONE Data in storage, nothing in wiki
        // - DONE Data in wiki and storage, within wiki max limit of 30 days
        // - TO-DO Test: Storage with data from 3 months <- this test should fail until more than 30 pageViewsForDays is properly supported
        // - TO-DO Deduplicate arrange logic of all tests
        [Test]
        public async Task NoData()
        {
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.MaxPageViewsForDays);

            new JsonDiffAssertion(new string[0], actualStats).Assert();
        }

        [Test]
        public async Task DataInWiki()
        {
            var wikiStats          = new ValidWikiPagesStatsFixture().PagesStats(UtcNowDay);
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, wikiStats: wikiStats);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.MaxPageViewsForDays);

            new JsonDiffAssertion(wikiStats, actualStats).Assert();
        }

        [Test]
        public async Task DataInStorage()
        {
            var storedStats        = new ValidWikiPagesStatsFixture().PagesStats(new DateDay(UtcNowDay));
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, storedStats);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.MaxPageViewsForDays);

            new JsonDiffAssertion(storedStats, actualStats).Assert();
        }

        [Test]
        public async Task DataInWikiAndStorageWithinWikiMaxPageViewsForDays()
        {
            var pageViewsForDays   = AdoWiki.MaxPageViewsForDays;
            var fixture            = new ValidWikiPagesStatsFixture();
            var currStats          = fixture.PagesStats(UtcNowDay);
            var currStatsDaySpan   = (int) currStats.VisitedDaysSpan!;
            var prevStats          = fixture.PagesStats(UtcNowDay.AddDays(-pageViewsForDays + currStatsDaySpan));
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, storedStats: prevStats, wikiStats: currStats);

            Assert.That(
                currStatsDaySpan,
                Is.GreaterThanOrEqualTo(2),
                "Precondition violation: the arranged data has to have at least two days span " +
                "between first and last days with any visits to provide meaningful test data");
            Assert.That(
                prevStats.FirstDayWithAnyVisit,
                Is.GreaterThanOrEqualTo(UtcNowDay.AddDays(-pageViewsForDays + 1)),
                $"Precondition violation: the first day of arranged stats is so much in the past that " +
                $"a call to PageStats won't return it.");

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(prevStats.Merge(currStats), actualStats).Assert();
        }

        private static async Task<AdoWikiWithStorage> AdoWikiWithStorage(
            DateDay utcNow,
            ValidWikiPagesStats? storedStats = null,
            ValidWikiPagesStats? wikiStats = null)
        {
            var declare = new Declare();
            var storage = await AdoWikiPagesStatsStorageTests.AdoWikiPagesStatsStorage(declare, utcNow, storedStats);
            var adoWiki = new SimulatedAdoWiki(wikiStats ?? new ValidWikiPagesStats(WikiPageStats.EmptyArray));
            var wiki    = declare.AdoWikiWithStorage(adoWiki, storage);
            return wiki;
        }
    }
}