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
            var stats            = fixture.PagesStatsForMonth(UtcNowDay);
            var storedStats      = stats.Trim(UtcNowDay, -pageViewsForDays, 0);
            var adoDecl          = new AzureDevOpsTestsDeclare(new Declare());
            var storage          = await adoDecl.AdoWikiPagesStatsStorage(UtcNowDay, storedStats);

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
    }
}