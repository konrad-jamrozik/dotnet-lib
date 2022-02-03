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
/// kj2 ValidWikiPagesStats invariants should be explained in English
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
        DateDay startDay,
        DateDay endDay) : this(stats, new DaySpan(startDay, endDay))
    {
        // kja get rid of this ctor by fixing callers to call the delegated-to ctor instead.
    }

    // Note this setup of invariant checks in ctor has some problems.
    // Details here: https://github.com/dotnet/csharplang/issues/4453#issuecomment-782807066
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

    public static DateDay? FirstDayWithAnyViewStatic(IEnumerable<WikiPageStats> stats)
    {
        var minDatePerPage = stats
            .Where(ps => ps.DayStats.Any())
            .Select(ps => ps.DayStats.Min(ds => ds.Day))
            .ToList();
        return minDatePerPage.Any() ? new DateDay(minDatePerPage.Min()) : null;
    }

    public DateDay? LastDayWithAnyView => LastDayWithAnyViewStatic(this);

    public static DateDay? LastDayWithAnyViewStatic(IEnumerable<WikiPageStats> stats)
    {
        var maxDatePerPage = stats
            .Where(ps => ps.DayStats.Any())
            .Select(ps => ps.DayStats.Max(ds => ds.Day))
            .ToList();
        return maxDatePerPage.Any() ? new DateDay(maxDatePerPage.Max()) : null;
    }

    public int ViewedDaysSpan => LastDayWithAnyView != null
        ? (int) (LastDayWithAnyView - FirstDayWithAnyView!).TotalDays + 1
        : 0;

    // kja this should be a method on DaySpan type
    public int DaysSpan => (int) (DaySpan.EndDay - DaySpan.StartDay).TotalDays + 1;

    // kja this should be a method on DaySpan type
    public int MonthsSpan => DateMonth.Range(DaySpan.StartDay, DaySpan.EndDay).Length;

    public IEnumerator<WikiPageStats> GetEnumerator() => Data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static ValidWikiPagesStats Merge(IEnumerable<ValidWikiPagesStats> stats, bool allowGaps = false) 
        // kj2 This Merge is O(n^2) while it could be O(n).
        => stats.Aggregate((merged, next) => merged.Merge(next, allowGaps));

    private static void CheckInvariants(
        IEnumerable<WikiPageStats> pagesStats,
        DaySpan dateSpan)
    {
        var pagesStatsArray = pagesStats as WikiPageStats[] ?? pagesStats.ToArray();
        pagesStatsArray.AssertDistinctBy(ps => ps.Id); 
        // Pages are expected to generally have unique paths, except the special case of when
        // a page was deleted and then new page was created with the same path.
        // In such case two pages with different IDs will have the same path.
        // kj2 test for ValidWikiPagesStats invariant; write a test that would fail if pagesStatsArray.AssertDistinctBy(ps => ps.Path); would be present.
        Contract.Assert(pagesStatsArray.DistinctBy(ps => ps.Path).Count() <= pagesStatsArray.Length);
        pagesStatsArray.AssertOrderedBy(ps => ps.Id);

        // kj2 move ValidWikiPagesStats invariants to WikiPageStats itself.
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

        Contract.Assert(dateSpan.StartDay.CompareTo(dateSpan.EndDay) <= 0);
        var firstDayWithAnyView = FirstDayWithAnyViewStatic(pagesStatsArray);
        var lastDayWithAnyView = LastDayWithAnyViewStatic(pagesStatsArray);

        // @formatter:off
        Contract.Assert(firstDayWithAnyView == null || dateSpan.StartDay.CompareTo(firstDayWithAnyView) <= 0);
        Contract.Assert(lastDayWithAnyView  == null || lastDayWithAnyView.CompareTo(dateSpan.EndDay   ) <= 0);
        // @formatter:on
    }

    public IEnumerable<ValidWikiPagesStatsForMonth> SplitByMonth() => SplitByMonth(this);

    private static IEnumerable<ValidWikiPagesStatsForMonth> SplitByMonth(ValidWikiPagesStats stats) 
    {
        // kja work in this method on DaySpans instead of start,end day pair.
        DateDay startDay = stats.DaySpan.StartDay;
        DateDay endDay = stats.DaySpan.EndDay;

        Contract.Assert(startDay.CompareTo(endDay) <= 0);

        var monthsRange = DateMonth.Range(startDay, endDay);
        List<ValidWikiPagesStatsForMonth> monthsStats =
            monthsRange.Length == 1
                ? new List<ValidWikiPagesStatsForMonth> { new(stats.Trim(startDay, endDay)) }
                : BuildMonthsStats(stats, monthsRange, startDay, endDay);
            
        Contract.Assert(monthsStats.First().DaySpan.StartDay == startDay);
        Contract.Assert(monthsStats.Last().DaySpan.EndDay == endDay);
        Contract.Assert(
            monthsStats.Count == 1 ||
            monthsStats.Skip(1).SkipLast(1).All(monthStats => monthStats.DaySpanIsForEntireMonth));

        return monthsStats;
    }

    private static List<ValidWikiPagesStatsForMonth> BuildMonthsStats(
        ValidWikiPagesStats stats,
        DateMonth[] monthsRange,
        DateDay startDay,
        DateDay endDay)
    {
        var firstMonthStats = new ValidWikiPagesStatsForMonth(stats.Trim(startDay, monthsRange.First().LastDay));
        var lastMonthStats = new ValidWikiPagesStatsForMonth(stats.Trim(monthsRange.Last().FirstDay, endDay));
        var middleMonths = monthsRange.Skip(1).SkipLast(1).ToList();
        var middleMonthsStats = middleMonths
            .Select(month => new ValidWikiPagesStatsForMonth(stats.Trim(month)))
            .ToList();
        var monthsStats = firstMonthStats.InList().Concat(middleMonthsStats).Append(lastMonthStats).ToList();
        return monthsStats;
    }

    public (ValidWikiPagesStatsForMonth? previousMonthStats, ValidWikiPagesStatsForMonth currentMonthStats)
        SplitIntoTwoMonths() => SplitIntoTwoMonths(this);

    private static (ValidWikiPagesStatsForMonth? previousMonthStats, ValidWikiPagesStatsForMonth currentMonthStats)
        SplitIntoTwoMonths(ValidWikiPagesStats stats)
    {
        if (stats.DaySpanIsWithinOneMonth())
            return (null, new ValidWikiPagesStatsForMonth(stats));

        // kja work in this method on DaySpans instead of start,end day pair.

        var startMonth = stats.DaySpan.StartDay.AsDateMonth();
        var endMonth = stats.DaySpan.EndDay.AsDateMonth();
        Contract.Assert(startMonth.NextMonth == endMonth,
            "Assert: at this point of execution the day span of stats being split into two months " +
            "is expected to span 2 months.");

        var splitMonths = stats.SplitByMonth().ToArray();

        Contract.Assert(splitMonths.Length == 2);
        Contract.Assert(splitMonths.First().DaySpan.StartDay == stats.DaySpan.StartDay);
        Contract.Assert(splitMonths.First().DaySpan.EndDay == stats.DaySpan.StartDay.AsDateMonth().LastDay);
        Contract.Assert(splitMonths.Last().DaySpan.StartDay == stats.DaySpan.EndDay.AsDateMonth().FirstDay);
        Contract.Assert(splitMonths.Last().DaySpan.EndDay == stats.DaySpan.EndDay);

        return (splitMonths.First(), splitMonths.Last());
    }

    public ValidWikiPagesStats Merge(ValidWikiPagesStats validCurrentStats, bool allowGaps = false)
        => Merge(this, validCurrentStats, allowGaps);

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
    // kj2 add tests showing this is associative but not commutative (not comm due to ignoring previousPageStats.Path and same with count)
    // Maybe also add some invariant check if a merge like next.merge(prev) is done instead of prev.merge(next). Basically the non-checked one mentioned above.
    private static ValidWikiPagesStats Merge(
        ValidWikiPagesStats previousStats,
        ValidWikiPagesStats currentStats,
        bool allowGaps)
    {
        IEnumerable<WikiPageStats> merged = previousStats.UnionMerge(currentStats, ps => ps.Id, Merge);
        merged = merged.OrderBy(ps => ps.Id)
            .Select(ps => ps with { DayStats = ps.DayStats.OrderBy(ds => ds.Day).ToArray() });

        // kja work in this method on DaySpans instead of start,end day pair.

        Contract.Assert(previousStats.DaySpan.StartDay.CompareTo(currentStats.DaySpan.StartDay) <= 0,
            "Assert: Previous stats range should start no later than current stats range");
        Contract.Assert(previousStats.DaySpan.EndDay.CompareTo(currentStats.DaySpan.EndDay) <= 0,
            "Assert: Previous stats range should end no later than current stats range");
        if (!allowGaps) 
            Contract.Assert(previousStats.DaySpan.EndDay.AddDays(1).CompareTo(currentStats.DaySpan.StartDay) >= 0,
                "Assert: There should be no gap in the previous stats range and current stats range");
        return new ValidWikiPagesStats(merged, previousStats.DaySpan.StartDay, currentStats.DaySpan.EndDay);
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
                $"Instead got: {dayStats.Last().Count} >= {dayStats.First().Count}. For day: {dayStats.Key.ToShortDateString()}");
            return dayStats.Last();
        }).ToArray();
        return mergedStats;
    }

    public ValidWikiPagesStats Trim(DateMonth month) => Trim(month.FirstDay, month.LastDay);

    public ValidWikiPagesStats TrimUntil(DateMonth month) => Trim(DateTime.MinValue, month.LastDay);

    public ValidWikiPagesStats TrimFrom(DateMonth month) => Trim(month.FirstDay, DateTime.MaxValue);

    public ValidWikiPagesStats Trim(DateTime currentDate, int daysFrom, int daysTo) => Trim(
        currentDate.AddDays(daysFrom),
        currentDate.AddDays(daysTo));

    public ValidWikiPagesStats Trim(DateTime startDate, DateTime endDate)
        => Trim(this, new DateDay(startDate), new DateDay(endDate));

    public ValidWikiPagesStats Trim(DateDay startDay, DateDay endDay)
        => Trim(this, startDay, endDay);

    private static ValidWikiPagesStats Trim(ValidWikiPagesStats stats, DateDay startDay, DateDay endDay) =>
        new ValidWikiPagesStats(stats.Select(ps =>
                ps with { DayStats = ps.DayStats.Where(s => s.Day >= startDay && s.Day <= endDay).ToArray() })
            .ToArray(), startDay, endDay);

    // kja this should be a method on DaySpan instead.
    public bool DaySpanIsWithinOneMonth() 
        => DaySpan.StartDay.AsDateMonth() == DaySpan.EndDay.AsDateMonth();
}