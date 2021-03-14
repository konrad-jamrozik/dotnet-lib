using System.Threading.Tasks;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
using Xunit;

namespace Wikitools.AzureDevOps.Tests
{
    public class AdoWikiWithStorageTests
    {
        // kja curr work: ObtainsDataFromAdoApiAndStorage
        [Fact]
        public async Task ObtainsDataFromAdoApiAndStorage()
        {
            // kja design the test in a way the same "Act & Assert" can be run with following Arrange:
            // - everything simulated
            // - ADO API real, the rest simulated
            // - file system storage and timeline real, the rest simulated
            // - everything real

            var wikiPagesStats = new WikiPageStats[] {};

            var pageViewsForDays = 30;
            
            var storageDirPath   = "S:/simulatedStorageDir";
            var adoWikiUri       = "unused://unused";
            var patEnvVar        = "unused";

            var fileSystem = new SimulatedOS().FileSystem;
            var timeline   = new SimulatedTimeline();
            var adoApi     = new SimulatedAdoApi(wikiPagesStats);

            var wiki = AdoWikiWithStorage(adoApi, adoWikiUri, patEnvVar, fileSystem, storageDirPath, timeline);

            // Act
            var pagesStats = await wiki.PagesStats(pageViewsForDays);
        }

        private static AdoWikiWithStorage AdoWikiWithStorage(
            SimulatedAdoApi adoApi,
            string adoWikiUri,
            string patEnvVar,
            IFileSystem fileSystem,
            string storageDirPath,
            SimulatedTimeline timeline)
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