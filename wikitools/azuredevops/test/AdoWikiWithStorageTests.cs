using System.Threading.Tasks;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.AzureDevOps.Tests
{
    public class AdoWikiWithStorageTests
    {
        // kja simulated tests
        // - Everything empty: no data from wiki, no data from storage
        // - Data in wiki, nothing in storage
        // - Storage with data from 3 months
        [Fact]
        public async Task NoData()
        {
            var wikiPagesStats = new WikiPageStats[] {};

            var pageViewsForDays = 30;

            var storageDirPath = "S:/simulatedStorageDir";
            var adoWikiUri     = "unused://unused";
            var patEnvVar      = "unused";

            var fileSystem = new SimulatedOS().FileSystem;
            var timeline   = new SimulatedTimeline();
            var adoApi     = new SimulatedAdoApi(wikiPagesStats);

            var wiki = AdoWikiWithStorage(
                AdoWiki(adoApi, adoWikiUri, patEnvVar), 
                Storage(fileSystem, storageDirPath, timeline));

            // Act
            var pagesStats = await wiki.PagesStats(pageViewsForDays);
        }

        /// <summary>
        /// This test tests the following:
        /// - ADO API for Wiki can be successfully queried for data
        /// - The obtained data can be successfully stored
        /// - The stored data is then properly merged into ADO API for wiki data
        /// when "wiki with storage" is used to obtain ADO wiki data both from
        /// the API and from the data saved in storage.
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
        public async Task ObtainsDataFromAdoApiAndStorage()
        {
            var windowsOS  = new WindowsOS();
            var fileSystem = windowsOS.FileSystem;
            var timeline   = new Timeline();
            var adoApi     = new AdoApi(windowsOS.Environment);

            // kja circular dependency: azuredevops-tests should not depend on wikitools
            // Also, make it use separate dir, for integration tests
            var cfg = WikitoolsConfig.From(fileSystem, "wikitools_config.json");

            var storageDirPath = cfg.TestStorageDirPath;
            var adoWikiUri     = cfg.AdoWikiUri;
            var patEnvVar      = cfg.AdoPatEnvVar;

            var adoWiki = AdoWiki(adoApi, adoWikiUri, patEnvVar);

            // Act 1. Obtain 10 days of page stats from wiki (days 1 to 10)
            var statsForDays1To10 = await adoWiki.PagesStats(pageViewsForDays: 10);
            
            // Act 2. Obtain 6 days of page stats from wiki (days 5 to 10)
            var statsForDays5To10 = await adoWiki.PagesStats(pageViewsForDays: 6);

            var statsForDays3To6 = statsForDays1To10.Trim(timeline.UtcNow.Trim(DateTimePrecision.Day), -7, -4);
            var storage          = Storage(fileSystem, storageDirPath, timeline);

            // Act 3. Save to storage page stats days 3 to 6
            var storageWithStats = await storage.DeleteExistingAndSave(statsForDays3To6, timeline.UtcNow);

            // Act 4. Obtain last 8 days, with last 4 days of page stats from wiki.
            // kja To test what I want, I need to pass pageViewsForDays to 8, but set the wiki limit constant from 30 to 3.
            var statsForDays3To10 = await AdoWikiWithStorage(adoWiki, storageWithStats).PagesStats(pageViewsForDays: 8);

            // Assert 4.1. Corresponds to page stats days of 3 to 10 (storage 3 to 6 merged with API 7 to 10)
            var expected = statsForDays3To6.Merge(statsForDays5To10);
            new JsonDiffAssertion(expected, statsForDays3To10).Assert();
        }

        private static AdoWikiWithStorage AdoWikiWithStorage(
            AdoWiki adoWiki,
            WikiPagesStatsStorage storage) 
            => new(adoWiki, storage);

        private static WikiPagesStatsStorage Storage(IFileSystem fileSystem, string storageDirPath, ITimeline timeline) 
            => new(
                new MonthlyJsonFilesStorage(fileSystem, storageDirPath),
                timeline.UtcNow);

        private static AdoWiki AdoWiki(
            IAdoApi adoApi,
            string adoWikiUri,
            string patEnvVar)
            => new(adoApi, new AdoWikiUri(adoWikiUri), patEnvVar);
    }
}