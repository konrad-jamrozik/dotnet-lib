using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;
using static Wikitools.Lib.Primitives.SimulatedTimeline;

namespace Wikitools.AzureDevOps.Tests
{
    [TestFixture]
    public class AdoWikiWithStorageTests
    {
        // kja 3 test todos:
        // - DONE Everything empty: no data from wiki, no data from storage
        // - DONE Data in wiki, nothing in storage
        // - DONE Data in storage, nothing in wiki
        // - DONE Data in wiki and storage, within wiki max limit of 30 days
        // - DONE Deduplicate arrange logic of all tests
        // - TO-DO Test: Storage with data from 3 months <- this test should fail until more than 30 pageViewsForDays is properly supported
        [Test]
        public async Task NoData()
        {
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.MaxPageViewsForDays);

            new JsonDiffAssertion(new string[0], actualStats).Assert();
        }

        [Test]
        public async Task DataInWiki()
        {
            var wikiStats          = new ValidWikiPagesStatsFixture().PagesStats(UtcNowDay);
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, wikiStats: wikiStats);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.MaxPageViewsForDays);

            new JsonDiffAssertion(wikiStats, actualStats).Assert();
        }

        [Test]
        public async Task DataInStorage()
        {
            var storedStats        = new ValidWikiPagesStatsFixture().PagesStatsForMonth(new DateDay(UtcNowDay));
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, storedStats);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.MaxPageViewsForDays);

            new JsonDiffAssertion(storedStats, actualStats).Assert();
        }

        /// <summary>
        /// Given
        /// - wiki page stats for current month coming from wiki via API
        /// - and wiki page stats for previous month coming from storage,
        ///   starting from the earliest available day in the AdoWiki.MaxPageViewsForDays window
        /// When
        /// - querying AdoWikiWithStorage for page stats for the entire time window
        ///   of AdoWiki.MaxPageViewsForDays
        /// Then
        /// - return the union of both previous stats (coming from storage) and current stats (coming from wiki).
        /// </summary>
        [Test]
        public async Task DataInWikiAndStorageWithinWikiMaxPageViewsForDays()
        {
            var pageViewsForDays   = AdoWiki.MaxPageViewsForDays;
            var fix                = new ValidWikiPagesStatsFixture();
            var currStats          = fix.PagesStatsForMonth(UtcNowDay);
            var currStatsDaySpan   = (int) currStats.VisitedDaysSpan!;
            var prevStats          = fix.PagesStatsForMonth(UtcNowDay.AddDays(-pageViewsForDays + currStatsDaySpan));
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, storedStats: prevStats, wikiStats: currStats);

            Assert.That(
                currStatsDaySpan,
                Is.GreaterThanOrEqualTo(2),
                "Precondition violation: the arranged data has to have at least two days span " +
                "between first and last days with any visits to provide meaningful test data");
            Assert.That(
                prevStats.FirstDayWithAnyVisit,
                Is.GreaterThanOrEqualTo(UtcNowDay.AddDays(-pageViewsForDays + 1)),
                "Precondition violation: the first day of arranged stats is so much in the past that " +
                "a call to PageStats won't return it.");
            Assert.That(
                prevStats.Month,
                Is.Not.EqualTo(currStats.Month),
                "Precondition violation: previous month (stored) is different from current month (from wiki)");

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(prevStats.Merge(currStats), actualStats).Assert();
        }

        // kja 4 WIP test to exercise 3 months of data
        // kja 4.1. Another test:
        // Stores 7 months, but 1st, 2nd, 4rd, 6th and 7th months have no day stats (7th == current)
        [Test]
        public async Task DataFromStorageOfMoreThanMaxPageViewForDays()
        {
            var fix = new ValidWikiPagesStatsFixture();

            var currMonthStats = fix.PagesStatsForMonth(UtcNowDay);
            var prevMonthStats = fix.PagesStatsForMonth(UtcNowDay.AddMonths(-1));
            var prevPrevMonthStats = fix.PagesStatsForMonth(UtcNowDay.AddMonths(-2));

            var allStats = prevPrevMonthStats.Merge(prevMonthStats).Merge(currMonthStats);

            // kja 5 call tree of this needs implementation of to-do 6.
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, storedStats: allStats);

            // kja 7 this is like Wikitools.AzureDevOps.ValidWikiPagesStats.VisitedDaysSpan but for multiple months
            // Similarly, Merge making "allStats" should be done for Month range. And also SplitByMonth from to-do 6.
            // Probably introduce some extension method on IEnumerable<ValidWikiPagesStats> for the day span
            // and for merges.
            int daySpan = (int) (currMonthStats.LastDayWithAnyVisit! - prevPrevMonthStats.FirstDayWithAnyVisit!).TotalDays + 1;

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(daySpan);

            new JsonDiffAssertion(allStats, actualStats).Assert();
        }

        private static Task<AdoWikiWithStorage> AdoWikiWithStorage(
            DateDay utcNow,
            ValidWikiPagesStatsForMonth storedStats,
            ValidWikiPagesStats? wikiStats = null)
            => AdoWikiWithStorage(utcNow, (ValidWikiPagesStats) storedStats, wikiStats);

        private static async Task<AdoWikiWithStorage> AdoWikiWithStorage(
            DateDay utcNow,
            ValidWikiPagesStats? storedStats = null,
            ValidWikiPagesStats? wikiStats = null)
        {
            var decl      = new AzureDevOpsDeclare();
            var testsDecl = new AzureDevOpsTestsDeclare(decl);
            var storage   = await testsDecl.AdoWikiPagesStatsStorage(utcNow, storedStats);
            var adoWiki   = new SimulatedAdoWiki(wikiStats ?? new ValidWikiPagesStats(WikiPageStats.EmptyArray));
            var wiki      = decl.AdoWikiWithStorage(adoWiki, storage);
            return wiki;
        }
    }
}