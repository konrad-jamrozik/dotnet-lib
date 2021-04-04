using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;
using static Wikitools.Lib.Primitives.SimulatedTimeline;

namespace Wikitools.AzureDevOps.Tests
{
    [TestFixture]
    public class AdoWikiPagesStatsStorageTests
    {
        /// <summary>
        /// This test ensures that for "pageViewsForDays", the storage.PagesStats()
        /// correctly returns data for day range:
        /// [today - pageViewsForDays + 1, today]
        /// instead of the incorrect:
        /// [today - pageViewsForDays, today]
        /// </summary>
        [Test]
        public async Task FirstDayOfVisitsInStorageIsNotOffByOne()
        {
            var pageViewsForDays = 3;
            var fixture          = new ValidWikiPagesStatsFixture();
            var stats            = fixture.PagesStats(UtcNowDay);
            var storedStats      = stats.Trim(UtcNowDay, -pageViewsForDays, 0);
            var storage          = await AdoWikiPagesStatsStorage(new Declare(), UtcNowDay, storedStats);

            Assert.That(
                stats.FirstDayWithAnyVisit,
                Is.EqualTo(stats.LastDayWithAnyVisit?.AddDays(-pageViewsForDays)),
                "Precondition violation: the off by one error won't be detected by this test as there " +
                "are no visits in the off (== first) day in the arranged data.");

            // Act
            var actualStats = storage.PagesStats(pageViewsForDays);

            var expectedStats = storedStats.Trim(UtcNowDay, -pageViewsForDays+1, 0);
            new JsonDiffAssertion(expectedStats, actualStats).Assert();
        }

        public static async Task<AdoWikiPagesStatsStorage> AdoWikiPagesStatsStorage(
            Declare decl,
            DateDay utcNow,
            ValidWikiPagesStats? storedStats = null)
        {
            var fs         = new SimulatedFileSystem();
            var storageDir = fs.NextSimulatedDir();
            var storage    = decl.AdoWikiPagesStatsStorage(utcNow, storageDir);
            if (storedStats != null)
                storage = await storage.OverwriteWith(storedStats, utcNow);
            return storage;
        }
    }
}