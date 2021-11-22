using System.Threading.Tasks;
using NUnit.Framework;
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
        ///
        ///   [today - pageViewsForDays + 1, today]
        /// 
        /// e.g. for pageViewsForDays = 3 it will return [today - 2, today]
        /// 
        /// instead of the incorrect:
        /// 
        ///   [today - pageViewsForDays, today]
        /// 
        /// e.g for pageViewsForDays = 3 it would be [today - 3, today].
        ///
        /// This is achieved by arranging in storage stats from day "today-pageViewsForDays"
        /// and then showing that when calling storage.PagesStats(pageViewsForDays)
        /// the returned stats will start from "today-pageViewsForDays+1"
        /// instead of "today-pageViewsForDays".
        /// </summary>
        [Test]
        public async Task FirstDayOfVisitsInStorageIsNotOffByOne()
        {
            var pageViewsForDays = 3;
            var fixture          = new ValidWikiPagesStatsFixture();
            var stats            = fixture.PagesStatsForMonth(UtcNowDay);
            var storedStats      = stats.Trim(UtcNowDay, -pageViewsForDays, 0);
            var adoDecl          = new AzureDevOpsTestsDeclare();
            var storage          = await adoDecl.AdoWikiPagesStatsStorage(UtcNowDay, storedStats);

            Assert.That(
                stats.FirstDayWithAnyVisit,
                Is.LessThanOrEqualTo(stats.LastDayWithAnyVisit?.AddDays(-pageViewsForDays)),
                "Precondition violation: the off by one error won't be detected by this test as there " +
                "are no visits in the off (== first) day in the arranged data.");

            // Act
            var actualStats = storage.PagesStats(pageViewsForDays);

            var expectedStats = storedStats.Trim(UtcNowDay, -pageViewsForDays+1, 0);
            new JsonDiffAssertion(expectedStats, actualStats).Assert();
        }
    }
}