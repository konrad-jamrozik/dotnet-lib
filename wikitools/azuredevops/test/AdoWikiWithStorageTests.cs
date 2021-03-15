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
        /// - Obtain 10 days of page stats from wiki (days 1 to 10)
        /// - Obtain 5 days of page stats from wiki (days 6 to 10)
        /// - Save to storage page stats days 3 to 7
        /// - Obtain 5 days of page stats from "wiki with storage"
        ///   - Assert this corresponds to page stats days of 3 to 10
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
            var cfg = WikitoolsConfig.From(fileSystem, "wikitools_config.json");

            var storageDirPath = cfg.StorageDirPath;
            var adoWikiUri     = cfg.AdoWikiUri;
            var patEnvVar      = cfg.AdoPatEnvVar;

            var adoWiki = AdoWiki(adoApi, adoWikiUri, patEnvVar);

            // Act - Obtain 10 days of page stats from wiki (days 1 to 10)
            var statsFor10Days = await adoWiki.PagesStats(pageViewsForDays: 10);
            
            // Act - Obtain 5 days of page stats from wiki (days 6 to 10)
            var statsFor5Days = await adoWiki.PagesStats(pageViewsForDays: 5);

            var storage = Storage(fileSystem, storageDirPath, timeline);
            var statsForDays3To7 = statsFor10Days.Trim(timeline.UtcNow.AddDays(-8), timeline.UtcNow.AddDays(-3));

            // Act - Save to storage page stats days 3 to 7
            storage.Save(statsForDays3To7);

            var wiki = AdoWikiWithStorage(adoWiki, storage);

            // Act - Obtain 5 days of page stats from "wiki with storage"
            var pagesStats3To10 = await wiki.PagesStats(pageViewsForDays: 5);

            new JsonDiffAssertion(statsForDays3To7.Merge(statsFor5Days), pagesStats3To10).Assert();
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