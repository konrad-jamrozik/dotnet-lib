using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;

namespace Wikitools.AzureDevOps.Tests;

[TestFixture]
public class AdoWikiWithStorageTests
{
    [Test]
    public async Task NoData()
    {
        var today           = new SimulatedTimeline().UtcNowDay;
        var wikiWithStorage = await AdoWikiWithStorage(today);

        // Act
        var actualStats = await wikiWithStorage.PagesStats(PageViewsForDays.Max);

        new JsonDiffAssertion(new string[0], actualStats).Assert();
    }

    [Test]
    public async Task DataInWiki()
    {
        var today           = new SimulatedTimeline().UtcNowDay;
        var wikiStats       = new ValidWikiPagesStatsFixture().WikiPagesStats(today);
        var wikiWithStorage = await AdoWikiWithStorage(today, wikiStats: wikiStats);

        // Act
        var actualStats = await wikiWithStorage.PagesStats(PageViewsForDays.Max);

        new JsonDiffAssertion(wikiStats, actualStats).Assert();
    }

    [Test]
    public async Task DataInStorage()
    {
        var today           = new SimulatedTimeline().UtcNowDay;
        var storedStats     = new ValidWikiPagesStatsFixture().PagesStatsForMonth(new DateDay(today));
        var wikiWithStorage = await AdoWikiWithStorage(today, storedStats);

        // Act
        var actualStats = await wikiWithStorage.PagesStats(PageViewsForDays.Max);

        new JsonDiffAssertion(storedStats, actualStats).Assert();
    }

    /// <summary>
    /// Given
    /// - wiki page stats for current month coming from ADO wiki API
    /// - and wiki page stats for previous month coming from storage,
    ///   starting from the earliest valid day in the AdoWiki.MaxPageViewsForDays window
    /// When
    /// - querying AdoWikiWithStorage for page stats for the entire day span of PageViewsForDays.Max
    /// Then
    /// - the merged stats of both previous stats (coming from storage) and current stats (coming from wiki)
    ///   are returned.
    /// </summary>
    [Test]
    public async Task DataInWikiAndStorageWithinWikiPageViewsForDaysMax()
    {
        var today            = new SimulatedTimeline().UtcNowDay;
        var pvfd             = PageViewsForDays.Max;
        var pvfdDaySpan      = new PageViewsForDays(pvfd).AsDaySpanUntil(today);
        var fix              = new ValidWikiPagesStatsFixture();
        var currMonthStats   = fix.PagesStatsForMonth(today);
        var currMonthStatsDaySpan = currMonthStats.ViewedDaysSpan;

        Assert.That(
            currMonthStatsDaySpan,
            Is.GreaterThanOrEqualTo(2),
            "Precondition violation: the arranged data has to have at least two days span " +
            "between first and last days with any views to provide meaningful test data");

        // The prevMonthStatsShift is chosen in such a way that the input assumptions as captured
        // by assertions below are obeyed. The exact acceptable values depend on
        // currMonthStats content.
        DateDay prevMonthStatsShift = today.AddDays(-26);
        var prevMonthStats = fix.PagesStatsForMonth(prevMonthStatsShift)
            .TrimFrom(pvfdDaySpan.StartDay)
            .Trim(currMonthStats.Month.AddMonths(-1));
        
        Assert.That(
            prevMonthStats.FirstDayWithAnyView,
            Is.EqualTo(pvfdDaySpan.StartDay),
            "Precondition violation: the first day of arranged stats has to be exactly at the " +
            "start day boundary of PageViewsForDays.Max.");
        Assert.That(
            prevMonthStats.Month,
            Is.Not.EqualTo(currMonthStats.Month),
            "Precondition violation: previous month (stored) " +
            "is different from current month (from wiki)");

        var wikiWithStorage = await AdoWikiWithStorage(
            today,
            storedStats: prevMonthStats,
            wikiStats: currMonthStats);

        // Act 1/2
        var actualStats = await wikiWithStorage.PagesStats(pvfd);

        new JsonDiffAssertion(prevMonthStats.Merge(currMonthStats, allowGaps: true), actualStats).Assert();

        // Act 2/2
        // Same as Act 1/2, but instead of exercising PagesStats,
        // exercises PageStats for page with ID 1.
        actualStats = await wikiWithStorage.PageStats(pvfd, 1);

        new JsonDiffAssertion(
                prevMonthStats.WhereStats(ps => ps.Id == 1)
                    .Merge(
                        currMonthStats.WhereStats(ps => ps.Id == 1),
                        allowGaps: true), 
                actualStats)
            .Assert();
    }

    /// <summary>
    /// Given
    /// - wiki page stats that were stored earlier than PageViewsForDays.Max days ago,
    /// meaning they cannot be updated from the wiki, as they are beyond
    /// PageViewsForDays.Max days into the past.
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
        var today                = new SimulatedTimeline().UtcNowDay;
        var statsInMonthPresence = new[] { false, false, true, false, true, true, false, false };
        var storedStats = ArrangeStatsFromMonths(statsInMonthPresence);
        Assert.That(storedStats.ViewedDaysSpan > PageViewsForDays.Max);
        Assert.That(storedStats.DaySpan.Count > 6*31, "Should be more than 6 months");
        Assert.That(storedStats.DaySpan.MonthsCount == statsInMonthPresence.Length);

        var wikiWithStorage = await AdoWikiWithStorage(today, storedStats);

        // Act
        var actualStats = await wikiWithStorage.PagesStats(storedStats.DaySpan.Count);

        new JsonDiffAssertion(storedStats, actualStats).Assert();

        ValidWikiPagesStats ArrangeStatsFromMonths(bool[] pageStatsInMonthPresence)
        {
            var fix = new ValidWikiPagesStatsFixture();
            int monthsCount = pageStatsInMonthPresence.Length;
            var months = pageStatsInMonthPresence.Select(
                (statsPresent, i) =>
                {
                    int earliestMonthOffset = -(monthsCount - 1);
                    DateDay currDay = today.AddMonths(earliestMonthOffset + i);
                    return statsPresent
                        ? fix.PagesStatsForMonth(currDay)
                        : new ValidWikiPagesStatsForMonth(
                            WikiPageStats.EmptyArray,
                            new DaySpan(currDay));
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
        var storage     = await storageDecl.New(storedStats);
        var daySpan     = new DaySpan(utcNow);
        var httpClient =
            new SimulatedWikiHttpClient(
                wikiStats ?? new ValidWikiPagesStats(WikiPageStats.EmptyArray, daySpan),
                Today: daySpan.EndDay);
        var wiki = new AdoWiki(httpClient);
        var wikiWithStorage = wikiDecl.AdoWikiWithStorage(wiki, storage);
        return wikiWithStorage;
    }
}