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
        // - TO-DO wiki integration tests: see todos below
        // - TO-DO Test: Storage with data from 3 months <- this test should fail until more than 30 pageViewsForDays is properly supported
        // - TO-DO Deduplicate arrange logic of all tests
        // - TO-DO Break dependency on WikitoolsConfig.From(fs): this is forbidden dependency: azuredevops-tests should not depend on wikitools
        [Test]
        public async Task NoData()
        {
            var decl               = new TestDeclare();
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new SimulatedTimeline().UtcNow;
            var adoWiki            = new SimulatedAdoWiki(WikiPageStats.EmptyArray);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = decl.Storage(utcNow, storageDir);
            var adoWikiWithStorage = decl.AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = AdoWiki.MaxPageViewsForDays;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(new string[0], pagesStats).Assert();
        }

        [Test]
        public async Task DataInWiki()
        {
            var decl               = new TestDeclare();
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new SimulatedTimeline().UtcNow;
            var f                  = new ValidWikiPagesStatsFixture();
            var pagesStatsData     = f.PagesStats(new DateDay(utcNow));
            var adoWiki            = new SimulatedAdoWiki(pagesStatsData);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = decl.Storage(utcNow, storageDir);
            var adoWikiWithStorage = decl.AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = AdoWiki.MaxPageViewsForDays;;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(pagesStatsData, pagesStats).Assert();
        }

        [Test]
        public async Task DataInStorage()
        {
            var decl               = new TestDeclare();
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new SimulatedTimeline().UtcNow;
            var f                  = new ValidWikiPagesStatsFixture();
            var pagesStatsData     = f.PagesStats(new DateDay(utcNow));
            var adoWiki            = new SimulatedAdoWiki(WikiPageStats.EmptyArray);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = await decl.Storage(utcNow, storageDir).OverwriteWith(pagesStatsData, utcNow);
            var adoWikiWithStorage = decl.AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = AdoWiki.MaxPageViewsForDays;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(pagesStatsData, pagesStats).Assert();
        }

        [Test]
        public async Task DataInWikiAndStorageWithinWikiMaxPageViewsForDays()
        {
            var decl               = new TestDeclare();
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new DateDay(new SimulatedTimeline().UtcNow);
            var pageViewsForDays   = AdoWiki.MaxPageViewsForDays;
            var f                  = new ValidWikiPagesStatsFixture();
            var currStats          = f.PagesStats(utcNow);
            var currStatsDaySpan   = (int) currStats.VisitedDaysSpan!;
            var prevStats          = f.PagesStats(utcNow.AddDays(-pageViewsForDays + currStatsDaySpan));
            var expectedPagesStats = prevStats.Merge(currStats);
            var adoWiki            = new SimulatedAdoWiki(currStats);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = await decl.Storage(utcNow, storageDir).OverwriteWith(prevStats, utcNow);
            var adoWikiWithStorage = decl.AdoWikiWithStorage(adoWiki, storage);
            
            Assert.That(
                currStatsDaySpan,
                Is.GreaterThanOrEqualTo(2),
                "Precondition violation: the arranged data has to have at least two days span " +
                "between first and last days with any visits to provide meaningful test data");
            Assert.That(
                prevStats.FirstDayWithAnyVisit,
                Is.GreaterThanOrEqualTo(utcNow.AddDays(-pageViewsForDays + 1)),
                $"Precondition violation: the first day of arranged stats is so much in the past that " +
                $"a call to PageStats won't return it.");

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(expectedPagesStats, pagesStats).Assert();
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
            var decl             = new TestDeclare();
            var fs               = new SimulatedFileSystem();
            var utcNow           = new DateDay(new SimulatedTimeline().UtcNow);
            var pageViewsForDays = 3;
            var f                = new ValidWikiPagesStatsFixture();
            var stats            = f.PagesStats(utcNow);
            var storedStats      = stats.Trim(utcNow, -pageViewsForDays, 0);
            var storageDir       = fs.NextSimulatedDir();
            var storage          = await decl.Storage(utcNow, storageDir).OverwriteWith(storedStats, utcNow);

            Assert.That(
                stats.FirstDayWithAnyVisit,
                Is.LessThanOrEqualTo(stats.LastDayWithAnyVisit?.AddDays(-pageViewsForDays)),
                "Precondition violation: the off by one error won't be detected by this test as there " +
                "are no visits in the off (== first) day in the arranged data.");

            // Act
            var readStats = storage.PagesStats(pageViewsForDays);

            var expectedStats = storedStats.Trim(utcNow, -pageViewsForDays+1, 0);
            new JsonDiffAssertion(expectedStats, readStats).Assert();
        }
    }
}