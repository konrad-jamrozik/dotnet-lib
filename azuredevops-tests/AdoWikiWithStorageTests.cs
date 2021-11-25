using System.Linq;
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
        [Test]
        public async Task NoData()
        {
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.PageViewsForDaysMax);

            new JsonDiffAssertion(new string[0], actualStats).Assert();
        }

        [Test]
        public async Task DataInWiki()
        {
            var wikiStats          = new ValidWikiPagesStatsFixture().WikiPagesStats(UtcNowDay);
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, wikiStats: wikiStats);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.PageViewsForDaysMax);

            new JsonDiffAssertion(wikiStats, actualStats).Assert();
        }

        [Test]
        public async Task DataInStorage()
        {
            var storedStats        = new ValidWikiPagesStatsFixture().PagesStatsForMonth(new DateDay(UtcNowDay));
            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, storedStats);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(AdoWiki.PageViewsForDaysMax);

            new JsonDiffAssertion(storedStats, actualStats).Assert();
        }

        /// <summary>
        /// Given
        /// - wiki page stats for current month coming from wiki via API
        /// - and wiki page stats for previous month coming from storage,
        ///   starting from the earliest available day in the AdoWiki.MaxPageViewsForDays window
        /// When
        /// - querying AdoWikiWithStorage for page stats for the entire day span of AdoWiki.PageViewsForDaysMax
        /// Then
        /// - the merged stats of both previous stats (coming from storage) and current stats (coming from wiki)
        ///   are returned.
        /// </summary>
        [Test]
        public async Task DataInWikiAndStorageWithinWikiPageViewsForDaysMax()
        {
            var pageViewsForDays   = AdoWiki.PageViewsForDaysMax;
            var fix                = new ValidWikiPagesStatsFixture();
            var currStats          = fix.PagesStatsForMonth(UtcNowDay);
            var currStatsDaySpan   = currStats.VisitedDaysSpan;
            var prevStats = fix.PagesStatsForMonth(
                UtcNowDay.AddDays(-pageViewsForDays + currStatsDaySpan));
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

            new JsonDiffAssertion(prevStats.Merge(currStats, allowGaps: true), actualStats).Assert();
        }

        /// <summary>
        /// Given
        /// - wiki page stats that were stored earlier than AdoWiki.PageViewsForDaysMax days ago,
        /// meaning they cannot be updated from the wiki, as they are beyond
        /// AdoWiki.PageViewsForDaysMax days in the past.
        /// - and assuming the stats have the following characteristics:
        ///   - first stored month has no page visits at all
        ///   - last (current) stored month has no page visits at all
        ///   - there are months with stored visits
        ///   - and there is a "gap" month, i.e. a month chronologically in the middle of the stored
        ///     months that has no visits, but months before and after have visits.
        /// When
        /// - querying AdoWikiWithStorage for page stats for the entire day span of all the stored stats.
        /// Then
        /// - all stored stats are returned, merged.
        ///   - This means stats from beyond AdoWiki.PageViewsForDaysMax were included in the merged stats.
        ///   - This means the first and last months without any visits were not stripped, i.e.
        ///     their day span was included.
        /// </summary>
        [Test]
        public async Task DataFromStorageFromManyMonths()
        {
            var statsInMonthPresence = new[] { false, false, true, false, true, true, false, false };
            var storedStats = ArrangeStatsFromMonths(statsInMonthPresence);
            Assert.That(storedStats.VisitedDaysSpan > AdoWiki.PageViewsForDaysMax);
            Assert.That(storedStats.DaysSpan > 6*31, "Should be more than 6 months");
            Assert.That(storedStats.MonthsSpan == statsInMonthPresence.Length);

            var adoWikiWithStorage = await AdoWikiWithStorage(UtcNowDay, storedStats);

            // Act
            var actualStats = await adoWikiWithStorage.PagesStats(storedStats.DaysSpan);

            new JsonDiffAssertion(storedStats, actualStats).Assert();

            ValidWikiPagesStats ArrangeStatsFromMonths(bool[] pageStatsInMonthPresence)
            {
                var fix = new ValidWikiPagesStatsFixture();
                int monthsCount = pageStatsInMonthPresence.Length;
                var months = pageStatsInMonthPresence.Select(
                    (statsPresent, i) =>
                    {
                        DateDay currDay = UtcNowDay.AddMonths(-monthsCount + 1 + i);
                        return statsPresent
                            ? fix.PagesStatsForMonth(currDay)
                            : new ValidWikiPagesStatsForMonth(
                                WikiPageStats.EmptyArray,
                                startDay: currDay,
                                endDay: currDay);
                    });

                return ValidWikiPagesStats.Merge(months, allowGaps: true);
            }
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
            var wikiDecl    = new AdoWikiWithStorageDeclare();
            var storageDecl = new AdoWikiPagesStatsStorageDeclare();
            var storage     = await storageDecl.AdoWikiPagesStatsStorage(utcNow, storedStats);
            var adoWiki     = new SimulatedAdoWiki(
                wikiStats ?? new ValidWikiPagesStats(
                    WikiPageStats.EmptyArray,
                    startDay: utcNow,
                    endDay: utcNow));
            var wiki = wikiDecl.AdoWikiWithStorage(adoWiki, storage);
            return wiki;
        }
    }
}