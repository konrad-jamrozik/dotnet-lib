using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    /// <summary>
    /// Represents a collection of ADO wiki page stats, originally returned by "Page Stats - Get" [1]
    /// [1] https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/page%20stats/get?view=azure-devops-rest-6.0
    ///
    /// Assumed invariants about the underlying ADO API behavior, confirmed by manual tests:
    /// - Everything evident from the CheckInvariants() method.
    /// - If a page path was changed since last call to this method, it will appear only with the new path.
    ///   Consider a page with (ID, Path) of (42, "/Foo") and some set XDayViews of daily view counts.
    ///   Consider following sequence of events:
    ///   1. make first call to this method;
    ///   2. rename the page to /Bar;
    ///   3. make second call to this method.
    ///   In such case, we assume that:
    ///   - the result of the first call will show the page (42, "/Foo") with XDayViews
    ///   - the result of the second call will show (42, "/Bar") with XDayViews, and won't show (42, /"Foo") at all.
    /// - A page with the same path may appear under different ID in consecutive calls to this method.
    ///   - This can happen in case of page rename, as explained above.
    /// 
    /// </summary>
    public record ValidWikiPagesStats : IEnumerable<WikiPageStats>
    {
        // Note this setup of invariant checks in ctor has some problems.
        // Details here: https://github.com/dotnet/csharplang/issues/4453#issuecomment-782807066
        public ValidWikiPagesStats(IEnumerable<WikiPageStats> stats) => Data = CheckInvariants(stats);

        public ValidWikiPagesStats(ValidWikiPagesStats stats) => Data = stats;

        private static IEnumerable<WikiPageStats> CheckInvariants(IEnumerable<WikiPageStats> stats)
        {
            var statsArr = stats as WikiPageStats[] ?? stats.ToArray();
            statsArr.AssertDistinctBy(ps => ps.Id);
            statsArr.AssertDistinctBy(ps => ps.Path);
            statsArr.AssertOrderedBy(ps => ps.Id);
            statsArr.ForEach(ps =>
            {
                var (_, _, dayStats) = ps;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // This condition is written out here explicitly to clarify
                // that the length can be as low as 0.
                Contract.Assert(dayStats.Length >= 0);
                dayStats.AssertDistinctBy(ds => ds.Day);
                dayStats.AssertOrderedBy(ds => ds.Day);
                dayStats.Assert(ds => ds.Count >= 1);
                dayStats.Assert(ds => ds.Day.Kind == DateTimeKind.Utc);
            });
            return statsArr;
        }

        private IEnumerable<WikiPageStats> Data { get; }

        public (ValidWikiPagesStatsForMonth previousMonthStats, ValidWikiPagesStatsForMonth currentMonthStats)
            SplitByMonth(DateMonth currentMonth) => SplitByMonth(this, currentMonth);

        private static (ValidWikiPagesStatsForMonth previousMonthStats, ValidWikiPagesStatsForMonth currentMonthStats)
            SplitByMonth(
                ValidWikiPagesStats pagesStats,
                DateMonth currentMonth)
        {
            Contract.Assert(
                pagesStats.TrimUntil(currentMonth.PreviousMonth.PreviousMonth).VisitedDaysSpan == null,
                "The split stats have visits from before a month ago, which would be lost. " +
                $"I.e. from before month {currentMonth.PreviousMonth}.");
            Contract.Assert(
                pagesStats.TrimFrom(currentMonth.NextMonth).VisitedDaysSpan == null,
                "The split stats have visits from after current month, which would be lost. " +
                $"I.e. from after month {currentMonth}.");
            return (
                new ValidWikiPagesStatsForMonth(
                    pagesStats.Trim(currentMonth.PreviousMonth),
                    currentMonth.PreviousMonth),
                new ValidWikiPagesStatsForMonth(pagesStats.Trim(currentMonth), currentMonth)
            );
        }

        public ValidWikiPagesStats Merge(ValidWikiPagesStats validCurrentStats) => Merge(this, validCurrentStats);

        /// <summary>
        /// Merges ADO Wiki page stats. previousStats with currentStats.
        ///
        /// The merge makes following assumptions about the inputs.
        /// - The arguments obey ValidWikiPagesStats invariants, as evidenced by the argument types.
        /// - However, the union of arguments doesn't necessarily obey these invariants.
        /// Specifically:
        ///   - Page with the same ID might appear twice: once in each argument.
        ///   - Page with the same ID might appear under different paths.
        ///   - A day views stat for a page with given ID and Day date
        ///   can appear twice: once in each argument.
        /// 
        /// The stats returned by Merge obey the following:
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
        private static ValidWikiPagesStats Merge(ValidWikiPagesStats previousStats, ValidWikiPagesStats currentStats)
        {
            IEnumerable<WikiPageStats> merged = previousStats.UnionMerge(currentStats, ps => ps.Id, Merge);
            merged = merged.OrderBy(ps => ps.Id)
                .Select(ps => ps with { DayStats = ps.DayStats.OrderBy(ds => ds.Day).ToArray() });
            
            return new ValidWikiPagesStats(merged);
        }

        private static WikiPageStats Merge(WikiPageStats previousPageStats, WikiPageStats currentPageStats)
        {
            Contract.Assert(previousPageStats.Id == currentPageStats.Id);
            return new WikiPageStats(
                // You don't see previousPageStats.Path here as currentPageStats.Path takes precedence over it,
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
                    "Visit count for given day for given page cannot be lower in current stats, as compared to previous stats. " +
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

        public ValidWikiPagesStats Trim(DateTime startDate, DateTime endDate) => Trim(
            this,
            new DateDay(startDate),
            new DateDay(endDate));

        private static ValidWikiPagesStats Trim(ValidWikiPagesStats stats, DateDay startDay, DateDay endDay) =>
            new(stats.Select(ps =>
                    ps with { DayStats = ps.DayStats.Where(s => s.Day >= startDay && s.Day <= endDay).ToArray() })
                .ToArray());

        public IEnumerator<WikiPageStats> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public DateDay? FirstDayWithAnyVisit
        {
            get
            {
                var minDates = this
                    .Where(ps => ps.DayStats.Any())
                    .Select(s => s.DayStats.Min(ds => ds.Day))
                    .ToList();
                return minDates.Any() ? new DateDay(minDates.Min()) : null;
            }
        }

        public DateDay? LastDayWithAnyVisit
        {
            get
            {
                var maxDates = this
                    .Where(ps => ps.DayStats.Any())
                    .Select(s => s.DayStats.Max(ds => ds.Day))
                    .ToList();
                return maxDates.Any() ? new DateDay(maxDates.Max()) : null;
            }
        }

        public int? VisitedDaysSpan => LastDayWithAnyVisit != null 
            ? (int) (LastDayWithAnyVisit - FirstDayWithAnyVisit!).TotalDays + 1
            : null;

        public DateMonth MonthOfAllVisitedDays()
        {
            var firstDay = FirstDayWithAnyVisit;
            var lastDay  = LastDayWithAnyVisit;
            Contract.Assert(firstDay != null);
            Contract.Assert(firstDay!.Month == lastDay!.Month);
            return new DateMonth(firstDay);
        }

        public bool AllVisitedDaysAreInMonth(DateMonth month)
        {
            var firstDay = FirstDayWithAnyVisit;
            var lastDay = LastDayWithAnyVisit;
            return (firstDay?.Month == lastDay?.Month) && (firstDay == null || month.Equals(firstDay));
        }
    }
}