using System.Threading.Tasks;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
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

            var wiki = AdoWikiWithStorage(adoApi, adoWikiUri, patEnvVar, fileSystem, storageDirPath, timeline);

            // Act
            var pagesStats = await wiki.PagesStats(pageViewsForDays);
        }

        // kja integration test:
        // - Obtain 10 days from wiki (days 1 to 10)
        // - Obtain 5 days from wiki (days 6 to 10)
        // - Save to storage days 3 to 7
        // - Obtain 5 days from "wiki with storage"
        // - Assert the storage now has days 3 to 10
        [Trait("category", "integration")]
        [Fact]
        public async Task ObtainsDataFromAdoApiAndStorage()
        {
            var pageViewsForDays = 30;
            
            var windowsOS  = new WindowsOS();
            var fileSystem = windowsOS.FileSystem;
            var timeline   = new Timeline();
            var adoApi     = new AdoApi(windowsOS.Environment);

            // kja circular dependency: azuredevops-tests should not depend on wikitools
            var cfg = WikitoolsConfig.From(fileSystem, "wikitools_config.json");

            var storageDirPath = cfg.StorageDirPath;
            var adoWikiUri     = cfg.AdoWikiUri;
            var patEnvVar      = cfg.AdoPatEnvVar;

            var wiki = AdoWikiWithStorage(adoApi, adoWikiUri, patEnvVar, fileSystem, storageDirPath, timeline);

            // Act
            var pagesStats = await wiki.PagesStats(pageViewsForDays);
        }

        private static AdoWikiWithStorage AdoWikiWithStorage(
            IAdoApi adoApi,
            string adoWikiUri,
            string patEnvVar,
            IFileSystem fileSystem,
            string storageDirPath,
            ITimeline timeline)
        {
            var adoWiki = new AdoWiki(adoApi, new AdoWikiUri(adoWikiUri), patEnvVar);
            var storage = new WikiPagesStatsStorage(
                new MonthlyJsonFilesStorage(fileSystem, storageDirPath),
                timeline.UtcNow);
            var wiki = new AdoWikiWithStorage(adoWiki, storage);
            return wiki;
        }
    }
}