using System;
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
        // - TO DO Data in storage, nothing in wiki
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