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
    ///   2. rename the page, in the wiki, to /Bar;
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
        public ValidWikiPagesStats(
            IEnumerable<WikiPageStats> stats,
            DateDay statsRangeStartDay,
            DateDay statsRangeEndDay)
        {
            CheckInvariants(stats, statsRangeStartDay, statsRangeEndDay);
            Data = stats as WikiPageStats[] ?? stats.ToArray();
            StatsRangeStartDay = statsRangeStartDay;
            StatsRangeEndDay = statsRangeEndDay;
        }

        public DateDay StatsRangeStartDay { get; }
        public DateDay StatsRangeEndDay { get; }

        private IEnumerable<WikiPageStats> Data { get; }

        public DateDay? FirstDayWithAnyVisit => FirstDayWithAnyVisitStatic(this);

        public static DateDay? FirstDayWithAnyVisitStatic(IEnumerable<WikiPageStats> stats)
        {
            var minDatePerPage = stats
                .Where(ps => ps.DayStats.Any())
                .Select(ps => ps.DayStats.Min(ds => ds.Day))
                .ToList();
            return minDatePerPage.Any() ? new DateDay(minDatePerPage.Min()) : null;
        }

        public DateDay? LastDayWithAnyVisit => LastDayWithAnyVisitStatic(this);

        public static DateDay? LastDayWithAnyVisitStatic(IEnumerable<WikiPageStats> stats)
        {
            var maxDatePerPage = stats
                .Where(ps => ps.DayStats.Any())
                .Select(ps => ps.DayStats.Max(ds => ds.Day))
                .ToList();
            return maxDatePerPage.Any() ? new DateDay(maxDatePerPage.Max()) : null;
        }

        public int? VisitedDaysSpan => LastDayWithAnyVisit != null 
            ? (int) (LastDayWithAnyVisit - FirstDayWithAnyVisit!).TotalDays + 1
            : null;

        public IEnumerator<WikiPageStats> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static ValidWikiPagesStats Merge(params ValidWikiPagesStats[] stats)
            => Merge((IEnumerable<ValidWikiPagesStats>) stats);

        public static ValidWikiPagesStats Merge(IEnumerable<ValidWikiPagesStats> stats) 
            // kj2 This Merge is O(n^2) while it could be O(n).
            => stats.Aggregate((merged, next) => merged.Merge(next));

        private static void CheckInvariants(
            IEnumerable<WikiPageStats> stats, 
            DateDay statsRangeStartDay,
            DateDay statsRangeEndDay)
        {
            var statsArr = stats as WikiPageStats[] ?? stats.ToArray();
            statsArr.AssertDistinctBy(ps => ps.Id); 
            // Pages are expected to generally have unique paths, except the special case of when
            // a page was deleted and then new page was created with the same path.
            // In such case two pages with different IDs will have the same path.
            // kj2 test for this; write a test that would fail if statsArr.AssertDistinctBy(ps => ps.Path); would be present.
            Contract.Assert(statsArr.DistinctBy(ps => ps.Path).Count() <= statsArr.Length);
            statsArr.AssertOrderedBy(ps => ps.Id);

            // kj2 move these invariants to WikiPageStats itself.
            // Don't forget to update comment on that class!
            statsArr.ForEach(ps =>
            {
                var (_, _, dayStats) = ps;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // This condition is written out here explicitly to clarify
                // that the length can be as low as 0.
                Contract.Assert(dayStats.Length >= 0);
                // For each day, there is only stats entry for it.
                dayStats.AssertDistinctBy(ds => ds.Day);
                dayStats.AssertOrderedBy(ds => ds.Day);
                // Given page stats needs to have stats entry for at least one day.
                dayStats.Assert(ds => ds.Count >= 1); 
                dayStats.Assert(ds => ds.Day.Kind == DateTimeKind.Utc);
            });

            Contract.Assert(statsRangeStartDay.CompareTo(statsRangeEndDay) <= 0);
            var firstDayWithAnyVisit = FirstDayWithAnyVisitStatic(stats);
            Contract.Assert(firstDayWithAnyVisit == null || statsRangeStartDay.CompareTo(firstDayWithAnyVisit) <= 0);
            var lastDayWithAnyVisit = LastDayWithAnyVisitStatic(stats);
            Contract.Assert(lastDayWithAnyVisit == null || lastDayWithAnyVisit.CompareTo(statsRangeEndDay) <= 0);
        }

        public IEnumerable<ValidWikiPagesStatsForMonth> SplitByMonth(DateMonth currentMonth) 
        {
            // kja 6 to implement and test
            //
            // Once done, don't forget to remove the IF check at the beginning of
            // Wikitools.AzureDevOps.AdoWikiPagesStatsStorage.OverwriteWith
            // made for backward compat.
            //
            // Then also likely remove the 2-arg
            // Wikitools.AzureDevOps.AdoWikiPagesStatsStorage.OverwriteWith
            // as it seems to be used only in tests
            //
            // And then also rewrite the 2-month SplitByMonth to call this SplitByMonth,
            // but just with additional month checks for 2 months (these checks are there
            // to confirm data coming from wiki is indeed only from max 2 months).
            
            // kja 6.1 Also: ensure I have a test showing current month *has* to be passed.
            // If it wouldn't then we won't correctly handle situation in which current month
            // has no visits whatsoever. There is no way to conclude from the stats there is 
            // "extra empty month after the last month having any visits".
            // Also, if there are stats with no page visits at all, what would be the current month?
            
            // kja 6.2 What about first month? Can we deduce it? Is it a problem if not?
            // Devise some tests for it.
            // This assignment will throw NPE if the stats have no visits!
            DateDay startDate = FirstDayWithAnyVisit!;

            return DateMonth.Range(startDate, new DateDay(currentMonth))
                .Select(month => new ValidWikiPagesStatsForMonth(Trim(month)));
        }

        public (ValidWikiPagesStatsForMonth previousMonthStats, ValidWikiPagesStatsForMonth currentMonthStats)
            SplitIntoTwoMonths(DateMonth currentMonth) => SplitIntoTwoMonths(this, currentMonth);

        private static (ValidWikiPagesStatsForMonth previousMonthStats, ValidWikiPagesStatsForMonth currentMonthStats)
            SplitIntoTwoMonths(
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
        private static ValidWikiPagesStats Merge(ValidWikiPagesStats previousStats, ValidWikiPagesStats currentStats)
        {
            IEnumerable<WikiPageStats> merged = previousStats.UnionMerge(currentStats, ps => ps.Id, Merge);
            merged = merged.OrderBy(ps => ps.Id)
                .Select(ps => ps with { DayStats = ps.DayStats.OrderBy(ds => ds.Day).ToArray() });

            Contract.Assert(previousStats.StatsRangeStartDay.CompareTo(currentStats.StatsRangeStartDay) <= 0,
                "Assert: Previous stats range should start no later than current stats range");
            Contract.Assert(previousStats.StatsRangeEndDay.CompareTo(currentStats.StatsRangeEndDay) <= 0,
                "Assert: Previous stats range should end no later than current stats range");
            Contract.Assert(previousStats.StatsRangeEndDay.AddDays(1).CompareTo(currentStats.StatsRangeStartDay) >= 0,
                "Assert: There should be no gap in the previous stats range and current stats range");
            return new ValidWikiPagesStats(merged, previousStats.StatsRangeStartDay, currentStats.StatsRangeEndDay);
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
            new ValidWikiPagesStats(stats.Select(ps =>
                    ps with { DayStats = ps.DayStats.Where(s => s.Day >= startDay && s.Day <= endDay).ToArray() })
                .ToArray(), startDay, endDay);

        public DateMonth MonthOfAllVisitedDays()
        {
            var firstDay = StatsRangeStartDay;
            var lastDay = StatsRangeEndDay;
            Contract.Assert(firstDay.Month == lastDay.Month);
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