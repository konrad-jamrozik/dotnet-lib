using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

/// <summary>
/// Represents a collection of ADO wiki page stats, originally returned by "Page Stats - Get" [1]
/// [1] https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/page%20stats/get?view=azure-devops-rest-6.0
///
/// Assumed invariants about the underlying ADO API behavior, confirmed by manual tests, are as follows:
/// kj2-asserts ValidWikiPagesStats invariants should be explained in English
/// - Everything evident from the CheckInvariants() method. 
/// - If a page path was changed since last call to this method, it will appear only with the new path.
///   Consider a page with (ID, Path) of (42, "/Foo") and some set XDayViews of daily view counts.
///   Consider following sequence of events:
///   1. make first call to this method;
///   2. rename the page, in the wiki, to /Bar;
///   3. make second call to this method.
///   In such case, we assume that:
///   - the result of the first call will show the page (42, "/Foo") with XDayViews
///   - the result of the second call will show (42, "/Bar") with XDayViews, and won't show (42, /"Foo") at all.
/// - A page with the same path may appear under different ID in consecutive calls to this method.
///   - This can happen in case of page rename, as explained above.
/// - If a page was deleted since last call to this method, it will no longer show in the stats at all.
/// 
/// </summary>
public record ValidWikiPagesStats : IEnumerable<WikiPageStats>
{
    public ValidWikiPagesStats(
        IEnumerable<WikiPageStats> stats,
        DaySpan daySpan)
    {
        var statsArr = stats as WikiPageStats[] ?? stats.ToArray();
        CheckInvariants(statsArr, daySpan);
        Data = statsArr;
        DaySpan = daySpan;
    }

    public DaySpan DaySpan { get; }

    private IEnumerable<WikiPageStats> Data { get; }

    public bool AnyDayViewsPresent => FirstDayWithAnyView != null;

    public DateDay? FirstDayWithAnyView => FirstDayWithAnyViewStatic(this);

    public DateDay? LastDayWithAnyView => LastDayWithAnyViewStatic(this);

    public int ViewedDaysSpan => LastDayWithAnyView != null
        ? (int) (LastDayWithAnyView - FirstDayWithAnyView!).TotalDays + 1
        : 0;

    public IEnumerator<WikiPageStats> GetEnumerator() => Data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static DateDay? FirstDayWithAnyViewStatic(IEnumerable<WikiPageStats> stats)
    {
        var firstDayWithAnyView = stats
            .Where(ps => ps.DayStats.Any())
            .Select(ps => ps.DayStats.Min(ds => ds.Day))
            .Min();
        return firstDayWithAnyView;
    }

    public static DateDay? LastDayWithAnyViewStatic(IEnumerable<WikiPageStats> stats)
    {
        var lastDayWithAnyView = stats
            .Where(ps => ps.DayStats.Any())
            .Select(ps => ps.DayStats.Max(ds => ds.Day))
            .Max();
        return lastDayWithAnyView;
    }

    public static ValidWikiPagesStats Merge(IEnumerable<ValidWikiPagesStats> stats, bool allowGaps = false) 
        // kja-perf This Merge is O(n^2) while it could be O(n).
        => stats.Aggregate((merged, next) => merged.Merge(next, allowGaps));

    public IEnumerable<ValidWikiPagesStatsForMonth> SplitByMonth() => SplitByMonth(this);

    public ValidWikiPagesStats WhereStats(Func<WikiPageStats, bool> predicate)
        => new ValidWikiPagesStats(Data.Where(predicate), DaySpan);

    public (ValidWikiPagesStatsForMonth? previousMonthStats, ValidWikiPagesStatsForMonth currentMonthStats)
        SplitIntoTwoMonths() => SplitIntoUpToTwoMonths(this);

    public ValidWikiPagesStats Merge(ValidWikiPagesStats validCurrentStats, bool allowGaps = false)
        => Merge(this, validCurrentStats, allowGaps);

    public ValidWikiPagesStatsForMonth Trim(DateMonth month) => new ValidWikiPagesStatsForMonth(Trim(month.DaySpan));

    public ValidWikiPagesStats TrimUntil(DateMonth month) => Trim(new DaySpan(DaySpan.StartDay, month.LastDay));

    public ValidWikiPagesStats TrimFrom(DateMonth month) => Trim(new DaySpan(month.FirstDay, DaySpan.EndDay));

    public ValidWikiPagesStats TrimFrom(DateDay day) => Trim(new DaySpan(day, DaySpan.EndDay));

    public ValidWikiPagesStats Trim(DateTime currentDate, int daysFrom, int daysTo) => Trim(
        currentDate.AddDays(daysFrom),
        currentDate.AddDays(daysTo));

    public ValidWikiPagesStats Trim(DateTime startDate, DateTime endDate)
        => Trim(new DateDay(startDate), new DateDay(endDate));

    public ValidWikiPagesStats Trim(DateDay startDay, DateDay endDay)
        => Trim(new DaySpan(startDay, endDay));

    public ValidWikiPagesStats Trim(DaySpan daySpan) => Trim(this, daySpan);

    private static void CheckInvariants(IEnumerable<WikiPageStats> pagesStats, DaySpan daySpan)
    {
        var pagesStatsArray = pagesStats as WikiPageStats[] ?? pagesStats.ToArray();
        pagesStatsArray.AssertDistinctBy(ps => ps.Id); 
        // Pages are expected to generally have unique paths, except the special case of when
        // a page was deleted and then new page was created with the same path.
        // In such case two pages with different IDs will have the same path.
        // kj2-tests test for ValidWikiPagesStats invariant; write a test that would fail if pagesStatsArray.AssertDistinctBy(ps => ps.Path); would be present.
        Contract.Assert(pagesStatsArray.DistinctBy(ps => ps.Path).Count() <= pagesStatsArray.Length);
        pagesStatsArray.AssertOrderedBy(ps => ps.Id);

        // kj2-asserts move ValidWikiPagesStats invariants to WikiPageStats itself.
        // Don't forget to update comment on that class!
        pagesStatsArray.ForEach(ps =>
        {
            var (_, _, dayStats) = ps;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // This condition is written out here explicitly to clarify
            // that the length can be as low as 0.
            Contract.Assert(dayStats.Length >= 0);
            // For each day, there is only one stats entry for it.
            dayStats.AssertDistinctBy(ds => ds.Day);
            dayStats.AssertOrderedBy(ds => ds.Day);
            // Given day has to be viewed at least once.
            dayStats.Assert(ds => ds.Count >= 1); 
            dayStats.Assert(ds => ds.Day.Kind == DateTimeKind.Utc);
        });

        Contract.Assert(daySpan.StartDay.CompareTo(daySpan.EndDay) <= 0);
        var firstDayWithAnyView = FirstDayWithAnyViewStatic(pagesStatsArray);
        var lastDayWithAnyView = LastDayWithAnyViewStatic(pagesStatsArray);

        // @formatter:off
        Contract.Assert(firstDayWithAnyView == null || daySpan.StartDay.CompareTo(firstDayWithAnyView) <= 0);
        Contract.Assert(lastDayWithAnyView  == null || lastDayWithAnyView.CompareTo(daySpan.EndDay   ) <= 0);
        // @formatter:on
    }

    private static IEnumerable<ValidWikiPagesStatsForMonth> SplitByMonth(ValidWikiPagesStats stats) 
    {
        var monthsStats = stats.DaySpan.IsWithinOneMonth
            ? new List<ValidWikiPagesStatsForMonth>{new(stats)}
            : MonthsStats(stats);

        Contract.Assert(monthsStats.First().DaySpan.StartDay == stats.DaySpan.StartDay);
        Contract.Assert(monthsStats.Last().DaySpan.EndDay == stats.DaySpan.EndDay);
        Contract.Assert(
            monthsStats.Skip(1).SkipLast(1).All(monthStats => monthStats.DaySpanIsForEntireMonth));

        return monthsStats;
    }

    private static List<ValidWikiPagesStatsForMonth> MonthsStats(ValidWikiPagesStats stats)
    {
        var monthsSpan = stats.DaySpan.MonthsSpan;
        Contract.Assert(monthsSpan.Length >= 2);

        var firstMonthStats =
            new ValidWikiPagesStatsForMonth(stats.TrimUntil(monthsSpan.First()));

        var middleMonths = monthsSpan.Skip(1).SkipLast(1).ToList();
        var middleMonthsStats = middleMonths
            .Select(month => new ValidWikiPagesStatsForMonth(stats.Trim(month)))
            .ToList();

        var lastMonthStats =
            new ValidWikiPagesStatsForMonth(stats.TrimFrom(monthsSpan.Last()));

        var monthsStats = 
            firstMonthStats.WrapInList()
            .Concat(middleMonthsStats)
            .Append(lastMonthStats)
            .ToList();

        return monthsStats;
    }

    private static (ValidWikiPagesStatsForMonth? previousMonthStats, ValidWikiPagesStatsForMonth currentMonthStats)
        SplitIntoUpToTwoMonths(ValidWikiPagesStats stats)
        => stats.DaySpan.IsWithinOneMonth
            ? (null, new ValidWikiPagesStatsForMonth(stats))
            : SplitIntoExactlyTwoMonths(stats);

    private static (ValidWikiPagesStatsForMonth? previousMonthStats, ValidWikiPagesStatsForMonth
        currentMonthStats) SplitIntoExactlyTwoMonths(ValidWikiPagesStats stats)
    {
        var monthsSpan = stats.DaySpan.MonthsSpan;
        Contract.Assert(monthsSpan.Length == 2);
        Contract.Assert(
            monthsSpan.First().NextMonth == monthsSpan.Last(),
            "Assert: at this point of execution the day span of stats being split into two months " +
            "is expected to span exactly 2 months.");

        var splitMonths = stats.SplitByMonth().ToArray();

        Contract.Assert(splitMonths.Length == 2);
        Contract.Assert(splitMonths.First().DaySpan.StartDay == stats.DaySpan.StartDay);
        Contract.Assert(splitMonths.First().DaySpan.EndDay == monthsSpan.First().LastDay);
        Contract.Assert(splitMonths.Last().DaySpan.StartDay == monthsSpan.Last().FirstDay);
        Contract.Assert(splitMonths.Last().DaySpan.EndDay == stats.DaySpan.EndDay);

        return (splitMonths.First(), splitMonths.Last());
    }

    /// <summary>
    /// Merges ADO Wiki page stats. previousStats with currentStats.
    ///
    /// The merge makes following assumptions about the inputs.
    /// - The arguments obey ValidWikiPagesStats invariants, as evidenced by the argument types.
    /// - The previous stats range is before current stats range, i.e. it starts no later and ends no later.
    /// - There is no gap in stats ranges, i.e. previous stats range ends no later than current stats range starts.
    /// - The union of arguments doesn't necessarily obey ValidWikiPagesStats invariants.
    /// Specifically:
    ///   - A page with the same ID might appear twice: once in each argument.
    ///   - A page with the same ID might appear under different paths.
    ///     - This happens when the page was renamed.
    ///   - A day views stat for a page with given ID and Day date
    ///     can appear twice: once in each argument.
    /// 
    /// The stats returned by Merge obey the following:
    /// - All constraints of ValidWikiPagesStats, as evidenced by the returned type.
    /// - Pages with the same ID are merged into one page.
    ///   - If the Paths were different, the Path from the currentStats is taken.
    ///     The Path from previousStats is discarded.
    ///     - This operates under the assumption the page was renamed,
    ///       and the currentStats are newer, i.e. after the rename.
    /// - A page with given ID has union of all day view stats from both arguments.
    /// - Day view stats for the same Day date for given page are merged
    ///   into one by taking the count from currentStats, and discarding the count
    ///   from previousStats.
    ///   - This operates under the assumption the currentStats are newer
    ///     and thus accumulated more views than prevStats.
    ///
    /// Following invariant is *not* checked. Calling this method with
    /// arguments violating this invariant is violating this method contract:
    /// - Dates of all day view stats for page with given ID in currentStats
    ///   are equal or more recent than all day view stats
    ///   for a page with the same ID in previousStats.
    /// </summary>
    // kj2-tests add tests showing this is associative but not commutative (not comm due to ignoring previousPageStats.Path and same with count)
    // Maybe also add some invariant check if a merge like next.merge(prev) is done instead of prev.merge(next). Basically the non-checked one mentioned above.
    private static ValidWikiPagesStats Merge(
        ValidWikiPagesStats previousStats,
        ValidWikiPagesStats currentStats,
        bool allowGaps)
    {
        IEnumerable<WikiPageStats> merged = previousStats.ConcatMerge(currentStats, ps => ps.Id, Merge);
        merged = merged.OrderBy(ps => ps.Id)
            .Select(ps => ps with { DayStats = ps.DayStats.OrderBy(ds => ds.Day).ToArray() });

        var mergedDaySpan = previousStats.DaySpan.Merge(currentStats.DaySpan, allowGaps);
        return new ValidWikiPagesStats(merged, mergedDaySpan);
    }

    private static WikiPageStats Merge(WikiPageStats previousPageStats, WikiPageStats currentPageStats)
    {
        Contract.Assert(previousPageStats.Id == currentPageStats.Id);
        return new WikiPageStats(
            // Passing currentPageStats.Path and ignoring previousPageStats.Path.
            // This is because currentPageStats.Path takes precedence,
            // to reflect any page Path changes that happened since previousPageStats were obtained.
            currentPageStats.Path, 
            previousPageStats.Id,
            Merge(previousPageStats.DayStats, currentPageStats.DayStats));
    }

    private static WikiPageStats.DayStat[] Merge(
        WikiPageStats.DayStat[] previousDayStats,
        WikiPageStats.DayStat[] currentDayStats)
    {
        var groupedByDay = previousDayStats.Concat(currentDayStats).GroupBy(stat => stat.Day);
        var mergedStats = groupedByDay.Select(dayStats =>
        {
            Contract.Assert(dayStats.Count(), "dayStats.Count()", new Range(1, 2),
                upperBoundReason: "Given day stat may appear max twice: once for previous stats and once for current stats");
            Contract.Assert(dayStats.Last().Count >= dayStats.First().Count,
                "View count for given day for given page cannot be lower in current stats, as compared to previous stats. " +
                "I.e. day stat count for current stats >= day stat count for previous stats. " +
                $"Instead got: {dayStats.Last().Count} >= {dayStats.First().Count}. " +
                $"For day: {((DateTime)dayStats.Key).ToShortDateString()}");
            return dayStats.Last();
        }).ToArray();
        return mergedStats;
    }

    private static ValidWikiPagesStats Trim(ValidWikiPagesStats stats, DaySpan daySpan) 
        => new ValidWikiPagesStats(stats.Select(ps =>
                ps with
                {
                    DayStats = ps.DayStats.Where(
                        s => s.Day >= daySpan.StartDay && s.Day <= daySpan.EndDay).ToArray()
                })
            .ToArray(), daySpan);

    public ValidWikiPagesStats Trim(int days)
        => Trim(days.AsDaySpanUntil(DaySpan.EndDay));

    public ValidWikiPagesStats TrimToPageId(int pageId)
    {
        var wikiPageStats = this.Single(pageStats => pageStats.Id == pageId).WrapInList();
        return new ValidWikiPagesStats(wikiPageStats, this.DaySpan);
    }

    public List<ValidWikiPagesStatsForMonth> PartitionByMonth()
    {
        // kja to implement
        return new List<ValidWikiPagesStatsForMonth>();
    }
}