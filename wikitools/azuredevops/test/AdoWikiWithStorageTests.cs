using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
using Wikitools.Lib.Tests.Json;
using Xunit;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.AzureDevOps.Tests
{
    public class AdoWikiWithStorageTests
    {
        // kja simulated tests
        // - DONE Everything empty: no data from wiki, no data from storage
        // - DONE Data in wiki, nothing in storage
        // - DONE Data in storage, nothing in wiki
        // - DONE Data in wiki and storage, within wiki max limit of 30 days
        // - TO DO Storage with data from 3 months <- this test should fail until more than 30 pageViewsForDays is properly supported
        [Fact]
        public async Task NoData()
        {
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new SimulatedTimeline().UtcNow;
            var adoWiki            = new SimulatedAdoWiki(WikiPageStats.EmptyArray);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = Storage(utcNow, storageDir);
            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = 30;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(new string[0], pagesStats).Assert();
        }

        [Fact] // kj2 dedup
        public async Task DataInWiki()
        {
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new SimulatedTimeline().UtcNow;
            var pagesStatsData     = ValidWikiPagesStatsFixture.PagesStats(new DateDay(utcNow));
            var adoWiki            = new SimulatedAdoWiki(pagesStatsData);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = Storage(utcNow, storageDir);
            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = 30;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(pagesStatsData, pagesStats).Assert();
        }

        [Fact] // kj2 dedup
        public async Task DataInStorage()
        {
            var fs             = new SimulatedFileSystem();
            var utcNow         = new SimulatedTimeline().UtcNow;
            var pagesStatsData = ValidWikiPagesStatsFixture.PagesStats(new DateDay(utcNow));
            var adoWiki        = new SimulatedAdoWiki(WikiPageStats.EmptyArray);
            var storageDir     = fs.NextSimulatedDir();
            var storage        = await Storage(utcNow, storageDir).DeleteExistingAndSave(pagesStatsData, utcNow);

            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = 30;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(pagesStatsData, pagesStats).Assert();
        }

        [Fact] // kj2 dedup
        public async Task DataInWikiAndStorageWithinSameMonth()
        {
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new SimulatedTimeline().UtcNow;
            var currStats = ValidWikiPagesStatsFixture.PagesStats(new DateDay(utcNow));
            // kja UPDATE: confirmed to be indeed wrong. See assertions in Wikitools.AzureDevOps.Tests.AdoWikiWithStorageTests.ObtainsAndStoredDataFromWiki
            // Original TO-DO:
            // Assuming here that the PagesStats goes up to 4 days in the past. But this seems to be off by one error.
            // That is, -27-4 = -31, so this should fail. If I increase pageViewsForDays to 31 it works.
            // The idea of pageViewsForDays == 30 is that it matches wiki behavior of max of 30 days.
            // How does wiki actually work? Will 30 days do today and 30 days in the past, or today and 29 days in the past?
            // If the first one we are good, if the second one, this test proves that adoWikiWithStorage.PagesStats(30)
            // includes one day too many.
            //
            // Solve this by creating an integration test for that, with pageViewsForDays = 1, and showing the actual
            // behavior: whether it is "today only" or "today and yesterday".
            var prevStats  = ValidWikiPagesStatsFixture.PagesStats(new DateDay(utcNow.AddDays(-27)));
            var expectedPagesStats = prevStats.Merge(currStats);
            var adoWiki            = new SimulatedAdoWiki(currStats);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = await Storage(utcNow, storageDir).DeleteExistingAndSave(prevStats, utcNow);

            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = 30;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(expectedPagesStats, pagesStats).Assert();
        }

        // kja curr work. Finish up the body (e.g. assert against stored stats) and move all the int tests to a separate class,
        // to deduplicate assumptions about the external wiki.
        /// <summary>
        /// This test tests the following:
        /// - ADO API for Wiki can be successfully queried for data
        /// - Querying wiki for 1 day results in it giving data for today only.
        /// - The obtained data can be successfully stored.
        ///
        /// External dependencies:
        /// Same as Wikitools.AzureDevOps.Tests.AdoWikiWithStorageTests.ObtainsAndMergesDataFromAdoWikiApiAndStorage
        /// </summary>
        [Trait("category", "integration")]
        [Fact]
        public async Task ObtainsAndStoredDataFromWiki()
        {
            var fs         = new FileSystem();
            var utcNow     = new Timeline().UtcNow;
            var env        = new Environment();
            var cfg        = WikitoolsConfig.From(fs); // kj2 forbidden dependency: azuredevops-tests should not depend on wikitools
            var storageDir = new Dir(fs, cfg.TestStorageDirPath);
            var storage    = Storage(utcNow, storageDir);
            var adoWiki    = new AdoWiki(cfg.AdoWikiUri, cfg.AdoPatEnvVar, env);

            // kja WEIRD: it doesn't work for 1 because apparently then minDay check has to be different?
            // Does the wiki return different stats for 2 and 1??
            // Maybe this is because it is 1:42 AM UTC and due to ingestion delays no stats for today have shown up yet.
            // kja I need a test proving that this ALWAYS returns empty day stats
            // And another test for exactly 2 days.
            var pageViewsForDays = 1;

            var wikiStats   = await adoWiki.PagesStats(pageViewsForDays: pageViewsForDays);
            var wikiStats2   = await adoWiki.PagesStats(pageViewsForDays: pageViewsForDays+1);
            // kja use for debugging
            // new JsonDiffAssertion(wikiStats, wikiStats2).Assert();
            var storedStats = await storage.DeleteExistingAndSave(wikiStats, utcNow);

            var minDay = wikiStats.Where(ps => ps.DayStats.Any()).Select(ps => ps.DayStats.Min(ds => ds.Day)).Min();
            var maxDay  = wikiStats.Where(ps => ps.DayStats.Any()).Select(s => s.DayStats.Max(ds => ds.Day)).Max();
            Assert.Equal(new DateDay(DateTime.UtcNow.AddDays(-1)), new DateDay(maxDay));
            // kja this will fail if the resource wiki was never visited on that day
            // kja the +1 here proves that the behavior does not match the stored stats behavior, i.e.
            // if the storage goes into past one day too many. This is the problem described in
            // Wikitools.AzureDevOps.Tests.AdoWikiWithStorageTests.DataInWikiAndStorageWithinSameMonth
            Assert.Equal(new DateDay(DateTime.UtcNow.AddDays(-pageViewsForDays+1)), new DateDay(minDay));
        }

        /// <summary>
        /// This test tests the following:
        /// - ADO API for Wiki can be successfully queried for data
        /// - The obtained data can be successfully stored
        /// - The stored data is then properly merged into ADO API for wiki data
        ///   when "wiki with storage" is used to obtain ADO wiki data both from
        ///   the API and from the data saved in storage.
        ///
        /// The tested scenario:
        /// 1. Obtain 10 days of page stats from wiki (days 1 to 10)
        /// 2. Obtain 6 days of page stats from wiki (days 5 to 10)
        /// 3. Save to storage page stats days 3 to 6
        /// 4. Obtain 4 days of page stats from "wiki with storage" (days 7 to 10)
        /// 4.1. Assert this corresponds to page stats days of 3 to 10 (storage 3 to 6 merged with API 7 to 10)
        ///
        /// External dependencies:
        /// This test queries whatever wiki is defined in WikitoolsConfig, using PAT read from
        /// Env var also defined in that config.
        /// Thus:
        /// - for this test to work, that wiki has to be accessible by the owner of the PAT
        /// - for this test to exercise meaningful behavior, there has to be recent ongoing, daily activity on
        /// the wiki.
        /// </summary>
        [Trait("category", "integration")]
        [Fact]
        public async Task ObtainsAndMergesDataFromAdoWikiApiAndStorage()
        {
            var fs         = new FileSystem();
            var utcNow     = new Timeline().UtcNow;
            var env        = new Environment();
            var cfg        = WikitoolsConfig.From(fs); // kj2 forbidden dependency: azuredevops-tests should not depend on wikitools
            var storageDir = new Dir(fs, cfg.TestStorageDirPath);
            var storage    = Storage(utcNow, storageDir);
            var adoWiki    = new AdoWiki(cfg.AdoWikiUri, cfg.AdoPatEnvVar, env);

            // Act 1. Obtain 10 days of page stats from wiki (days 1 to 10)
            var statsForDays1To10 = await adoWiki.PagesStats(pageViewsForDays: 10);

            // Act 2. Obtain 4 days of page stats from wiki (days 7 to 10)
            var statsForDays7To10 = await adoWiki.PagesStats(pageViewsForDays: 4);

            // Act 3. Save to storage page stats days 3 to 6
            var statsForDays3To6 = statsForDays1To10.Trim(utcNow, -7, -4);
            var storageWithStats = await storage.DeleteExistingAndSave(statsForDays3To6, utcNow);

            // Act 4. Obtain last 8 days, with last 4 days of page stats from wiki
            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storageWithStats, pageViewsForDaysWikiLimit: 4);
            var statsForDays3To10  = await adoWikiWithStorage.PagesStats(pageViewsForDays: 8);

            // Assert 4.1. Act 4 corresponds to page stats days of 3 to 10
            // (data from storage for days 3 to 6 merged with data from ADO API for days 7 to 10)
            var expected = statsForDays3To6.Merge(statsForDays7To10);
            new JsonDiffAssertion(expected, statsForDays3To10).Assert();
        }

        private static AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            WikiPagesStatsStorage storage,
            int? pageViewsForDaysWikiLimit = null)
            => new(adoWiki, storage, pageViewsForDaysWikiLimit);

        private static WikiPagesStatsStorage Storage(DateTime utcNow, Dir storageDir)
            => new(
                new MonthlyJsonFilesStorage(storageDir),
                utcNow);
    }
}