using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;

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
        // - TO-DO Break dependency on WikitoolsConfig.From(fs): this is forbidden dependency: azuredevops-tests should not depend on wikitools
        [Test]
        public async Task NoData()
        {
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNow);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.MaxPageViewsForDays);

            new JsonDiffAssertion(new string[0], actualStats).Assert();
        }

        [Test]
        public async Task DataInWiki()
        {
            var wikiStats          = new ValidWikiPagesStatsFixture().PagesStats(UtcNow);
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNow, wikiStats: wikiStats);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.MaxPageViewsForDays);

            new JsonDiffAssertion(wikiStats, actualStats).Assert();
        }

        [Test]
        public async Task DataInStorage()
        {
            var storedStats        = new ValidWikiPagesStatsFixture().PagesStats(new DateDay(UtcNow));
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNow, storedStats);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.MaxPageViewsForDays);

            new JsonDiffAssertion(storedStats, actualStats).Assert();
        }

        [Test]
        public async Task DataInWikiAndStorageWithinWikiMaxPageViewsForDays()
        {
            var pageViewsForDays   = AdoWiki.MaxPageViewsForDays;
            var fixture            = new ValidWikiPagesStatsFixture();
            var currStats          = fixture.PagesStats(UtcNow);
            var currStatsDaySpan   = (int) currStats.VisitedDaysSpan!;
            var prevStats          = fixture.PagesStats(UtcNow.AddDays(-pageViewsForDays + currStatsDaySpan));
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNow, storedStats: prevStats, wikiStats: currStats);

            Assert.That(
                currStatsDaySpan,
                Is.GreaterThanOrEqualTo(2),
                "Precondition violation: the arranged data has to have at least two days span " +
                "between first and last days with any visits to provide meaningful test data");
            Assert.That(
                prevStats.FirstDayWithAnyVisit,
                Is.GreaterThanOrEqualTo(UtcNow.AddDays(-pageViewsForDays + 1)),
                $"Precondition violation: the first day of arranged stats is so much in the past that " +
                $"a call to PageStats won't return it.");

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(prevStats.Merge(currStats), actualStats).Assert();
        }

        /// <summary>
        /// This test ensures that for "pageViewsForDays", the storage.PagesStats()
        /// correctly returns data for day range:
        /// [today - pageViewsForDays + 1, today]
        /// instead of the incorrect:
        /// [today - pageViewsForDays    , today]
        /// </summary>
        [Test]
        public async Task FirstDayOfVisitsInStorageIsNotOffByOne()
        {
            var pageViewsForDays = 3;
            var fixture          = new ValidWikiPagesStatsFixture();
            var stats            = fixture.PagesStats(UtcNow);
            var storedStats      = stats.Trim(UtcNow, -pageViewsForDays, 0);
            var storage          = await WikiPagesStatsStorage(UtcNow, storedStats);

            Assert.That(
                stats.FirstDayWithAnyVisit,
                Is.EqualTo(stats.LastDayWithAnyVisit?.AddDays(-pageViewsForDays)),
                "Precondition violation: the off by one error won't be detected by this test as there " +
                "are no visits in the off (== first) day in the arranged data.");

            // Act
            var actualStats = storage.PagesStats(pageViewsForDays);

            var expectedStats = storedStats.Trim(UtcNow, -pageViewsForDays+1, 0);
            new JsonDiffAssertion(expectedStats, actualStats).Assert();
        }

        private static DateDay UtcNow { get; } = new(new SimulatedTimeline().UtcNow);

        private static async Task<AdoWikiWithStorage> AdoWikiWithStorage(
            DateDay utcNow,
            ValidWikiPagesStats? storedStats = null,
            ValidWikiPagesStats? wikiStats = null)
        {
            var decl    = new TestDeclare();
            var storage = await Storage(decl, utcNow, storedStats);
            var adoWiki = new SimulatedAdoWiki(wikiStats ?? new ValidWikiPagesStats(WikiPageStats.EmptyArray));
            var wiki    = decl.AdoWikiWithStorage(adoWiki, storage);
            return wiki;
        }

        private static async Task<AdoWikiPagesStatsStorage> WikiPagesStatsStorage(
            DateDay utcNow,
            ValidWikiPagesStats storedStats)
        {
            var decl    = new TestDeclare();
            var storage = await Storage(decl, utcNow, storedStats);
            return storage;
        }

        private static async Task<AdoWikiPagesStatsStorage> Storage(
            TestDeclare decl,
            DateDay utcNow,
            ValidWikiPagesStats? storedStats = null)
        {
            var fs         = new SimulatedFileSystem();
            var storageDir = fs.NextSimulatedDir();
            var storage    = decl.Storage(utcNow, storageDir);
            if (storedStats != null)
                storage = await storage.OverwriteWith(storedStats, utcNow);
            return storage;
        }
    }
}