using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;
using static Wikitools.Lib.Primitives.SimulatedTimeline;

namespace Wikitools.AzureDevOps.Tests;

[TestFixture]
public class AdoWikiWithStorageTests
{
    [Test]
    public async Task NoData()
    {
        var wikiWithStorage = await AdoWikiWithStorage(UtcNowDay);

        // Act
        var actualStats = await wikiWithStorage.PagesStats(PageViewsForDays.Max);

        new JsonDiffAssertion(new string[0], actualStats).Assert();
    }

    [Test]
    public async Task DataInWiki()
    {
        var wikiStats       = new ValidWikiPagesStatsFixture().WikiPagesStats(UtcNowDay);
        var wikiWithStorage = await AdoWikiWithStorage(UtcNowDay, wikiStats: wikiStats);

        // Act
        var actualStats = await wikiWithStorage.PagesStats(PageViewsForDays.Max);

        new JsonDiffAssertion(wikiStats, actualStats).Assert();
    }

    [Test]
    public async Task DataInStorage()
    {
        var storedStats     = new ValidWikiPagesStatsFixture().PagesStatsForMonth(new DateDay(UtcNowDay));
        var wikiWithStorage = await AdoWikiWithStorage(UtcNowDay, storedStats);

        // Act
        var actualStats = await wikiWithStorage.PagesStats(PageViewsForDays.Max);

        new JsonDiffAssertion(storedStats, actualStats).Assert();
    }

    /// <summary>
    /// Given
    /// - wiki page stats for current month coming from wiki via API
    /// - and wiki page stats for previous month coming from storage,
    ///   starting from the earliest available day in the AdoWiki.MaxPageViewsForDays window
    /// When
    /// - querying AdoWikiWithStorage for page stats for the entire day span of PageViewsForDays.Max
    /// Then
    /// - the merged stats of both previous stats (coming from storage) and current stats (coming from wiki)
    ///   are returned.
    /// </summary>
    [Test]
    public async Task DataInWikiAndStorageWithinWikiPageViewsForDaysMax()
    {
        var pageViewsForDays   = PageViewsForDays.Max;
        var fix                = new ValidWikiPagesStatsFixture();
        var currStats          = fix.PagesStatsForMonth(UtcNowDay);
        var currStatsDaySpan   = currStats.ViewedDaysSpan;
        var prevStats = fix.PagesStatsForMonth(
            UtcNowDay.AddDays(-pageViewsForDays + currStatsDaySpan));
        var wikiWithStorage = await AdoWikiWithStorage(
            UtcNowDay,
            storedStats: prevStats,
            wikiStats: currStats);

        Assert.That(
            currStatsDaySpan,
            Is.GreaterThanOrEqualTo(2),
            "Precondition violation: the arranged data has to have at least two days span " +
            "between first and last days with any views to provide meaningful test data");
        Assert.That(
            prevStats.FirstDayWithAnyView,
            Is.GreaterThanOrEqualTo(UtcNowDay.AddDays(-pageViewsForDays + 1)),
            "Precondition violation: the first day of arranged stats is so much in the past that " +
            "a call to PageStats won't return it.");
        Assert.That(
            prevStats.Month,
            Is.Not.EqualTo(currStats.Month),
            "Precondition violation: previous month (stored) is different from current month (from wiki)");

        // Act
        var actualStats = await wikiWithStorage.PagesStats(pageViewsForDays);

        new JsonDiffAssertion(prevStats.Merge(currStats, allowGaps: true), actualStats).Assert();
    }

    /// <summary>
    /// Given
    /// - wiki page stats that were stored earlier than PageViewsForDays.Max days ago,
    /// meaning they cannot be updated from the wiki, as they are beyond
    /// PageViewsForDays.Max days in the past.
    /// - and assuming the stats have the following characteristics:
    ///   - first stored month has no page views at all
    ///   - last (current) stored month has no page views at all
    ///   - there are months with stored views
    ///   - and there is a "gap" month, i.e. a month chronologically in the middle of the stored
    ///     months that has no views, but months before and after have views.
    /// When
    /// - querying AdoWikiWithStorage for page stats for the entire day span of all the stored stats.
    /// Then
    /// - all stored stats are returned, merged.
    ///   - This means stats from beyond PageViewsForDays.Max were included in the merged stats.
    ///   - This means the first and last months without any views were not stripped, i.e.
    ///     their day span was included.
    /// </summary>
    [Test]
    public async Task DataFromStorageFromManyMonths()
    {
        var statsInMonthPresence = new[] { false, false, true, false, true, true, false, false };
        var storedStats = ArrangeStatsFromMonths(statsInMonthPresence);
        Assert.That(storedStats.ViewedDaysSpan > PageViewsForDays.Max);
        Assert.That(storedStats.DaysSpan > 6*31, "Should be more than 6 months");
        Assert.That(storedStats.MonthsSpan == statsInMonthPresence.Length);

        var wikiWithStorage = await AdoWikiWithStorage(UtcNowDay, storedStats);

        // Act
        var actualStats = await wikiWithStorage.PagesStats(storedStats.DaysSpan);

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
        var wiki     = new SimulatedAdoWiki(
            wikiStats ?? new ValidWikiPagesStats(WikiPageStats.EmptyArray, new DaySpan(utcNow)));
        var wikiWithStorage = wikiDecl.AdoWikiWithStorage(wiki, storage);
        return wikiWithStorage;
    }
}