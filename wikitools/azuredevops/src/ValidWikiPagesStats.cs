using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    /// <summary>
    /// Represents a collection of ADO wiki page stats.
    ///
    /// Assumed invariants about the underlying ADO API behavior, confirmed by manual tests:
    /// - All dates provided are in UTC.
    /// - For any given page, visit stats for given day appear only once.
    /// - For any given page, it might have an empty array of day visit stats.
    /// - For any given page, the day visit stats are ordered ascending by date.
    /// - For any day visit stats, its Count is int that is 1 or more.
    /// - No page ID will appear more than once.
    /// - As a consequence, for any given page, its (Path, ID) pair is unique within the scope
    ///   of one call to this method.
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
    public class ValidWikiPagesStats
    {
        // kja problems with invariant checks in ctor:
        // 1. eager, not lazy (maybe fix it by making .Value Lazy and putting it there)
        // 2. no distinction between required validation of inputs originating
        // outside of the system, and internal assertions that should always hold
        // Re 2.: If validation of external input fails, we want to throw
        // exception, expected during normal operation of the system.
        // However, after we do that once and obtain the type, any further
        // operations on the data and subsequent calls to this ctor might
        // or might not involve external inputs.
        // If no external inputs are involved, only internal ops, then
        // any violation of the invariants on subsequent construction 
        // should never happen and is an assertion failure.
        // Assertion failures may need to be conditionally skipped from compilation
        // in production usage for performance reasons.
        // Thus, ideally, we need to be able to do the following:
        // - Run these invariants as normal system validation, when ctoring
        // from external inputs.
        // - Run these invariants as assertions, that can be skipped from compilation
        // and result in hard failure, when using this ctor for internal logic checks.
        // This hints the invariant failure reporting logic should be decoupled from
        // how to handle the failures. For example, wrap the InvariantException
        // and rethrow further depending on the validation mode.
        public ValidWikiPagesStats(IEnumerable<WikiPageStats> stats)
        {
            var statsArr = stats.ToArray();

            statsArr.AssertDistinctBy(ps => ps.Id);
            statsArr.AssertDistinctBy(ps => ps.Path);
            statsArr.ForEach(ps =>
            {
                var (_, _, dayStats) = ps;
                dayStats.AssertDistinctBy(ds => ds.Day);
                dayStats.AssertOrderedBy(ds => ds.Day);
                dayStats.Assert(ds => ds.Count >= 1);
                dayStats.Assert(ds => ds.Day.Kind == DateTimeKind.Utc);
            });

            Value = statsArr;
        }

        // kja this should be private. This means that:
        // - serialization to HDD serializes this type directly
        // - assertions operate on this type directly
        // - reporting logic (data rows projection) cannot access this value directly (more on that in GetRows comment)
        public WikiPageStats[] Value { get; }

        public static ValidWikiPagesStats From(IEnumerable<WikiPageStats> stats) =>
            new(stats.Select(WikiPageStats.FixNulls));

        public ValidWikiPagesStats Merge(ValidWikiPagesStats validCurrentStats) => Merge(this, validCurrentStats);

        /// <summary>
        /// Merges ADO Wiki page stats. previousStats with currentStats.
        ///
        /// The merge makes following assumptions about the inputs.
        /// - The data within each of the two parameters obeys the constraints
        /// as outlined by the comment of
        /// Wikitools.AzureDevOps.AdoApi.GetAllWikiPagesDetails.
        /// - However, these constraints might be violated when considering
        /// the union of these arguments. Specifically:
        ///   - Page with the same ID might appear twice: once in each argument.
        ///   - Page with the same ID might appear under different paths.
        ///   - A day views stat for a page with given ID and Day date
        ///   can appear twice: once in each argument.
        ///     - In such case, the view count for currentStats is >=
        ///     of the view count of previousStats.
        /// 
        /// The merge guarantees the following: 
        /// The output will obey the constraints as outlined by
        /// the comment of 
        /// Wikitools.AzureDevOps.AdoApi.GetAllWikiPagesDetails.
        ///
        /// The merge behaves as follows:
        /// - Pages with the same ID are merged into one page.
        ///   - If the Paths were different, the Path from the currentStats
        ///   is taken and the Path from previousStats is discarded.
        ///     - This operations on the assumption the page was renamed,
        ///     and the currentStats are newer, i.e. after the rename.
        /// - A page with given ID will have union of all day view stats
        ///   from both arguments.
        /// - Day view stats for the same Day date for given page are merged
        ///   into one. Observe that we have assumed the Counts of the day
        ///   view stats were the same for both arguments.
        ///
        /// Following constraints are *not* enforced, and thus might be
        /// violated by the arguments:
        /// - dates of all day view stats for page with given ID in currentStats
        /// are equal or more recent than all day view stats for the same page
        /// in previousStats.
        /// </summary>
        private static ValidWikiPagesStats Merge(ValidWikiPagesStats validPreviousStats, ValidWikiPagesStats validCurrentStats)
        {
            var previousStats = validPreviousStats.Value;
            var currentStats  = validCurrentStats.Value;

            var previousStatsByPageId = previousStats.ToDictionary(ps => ps.Id);
            var currentStatsByPageId  = currentStats.ToDictionary(ps => ps.Id);

            var previousIds     = previousStats.Select(ps => ps.Id).ToHashSet();
            var currentIds      = currentStats.Select(ps => ps.Id).ToHashSet();
            var intersectingIds = previousIds.Intersect(currentIds).ToHashSet();

            var currentOnlyStats  = currentIds.Except(intersectingIds).Select(id => currentStatsByPageId[id]); // kja 2 test for this .Except in .UnionUsing (not covered)
            var previousOnlyStats = previousIds.Except(intersectingIds).Select(id => previousStatsByPageId[id]);
            var intersectingStats =
                intersectingIds.Select(id => Merge(previousStatsByPageId[id], currentStatsByPageId[id]));

            var merged = previousOnlyStats.Union(intersectingStats).Union(currentOnlyStats).ToArray();
            // kja 2 once Merge is tested, replace the set logic with UnionUsing
            var merged2 = previousStats.UnionUsing(currentStats, ps => ps.Id, Merge);

            merged = merged.Select(ps => ps with { DayStats = ps.DayStats.OrderBy(ds => ds.Day).ToArray() }).ToArray();

            Contract.Assert(merged.DistinctBy(m => m.Id).Count() == merged.Length, "Any given page appears only once");
            merged.ForEach(ps => Contract.Assert(
                ps.DayStats.DistinctBy(s => s.Day).Count() == ps.DayStats.Length,
                "There is only one stat per page per day"));

            return new ValidWikiPagesStats(merged);
        }

        private static WikiPageStats Merge(WikiPageStats previousPageStats, WikiPageStats currentPageStats)
        {
            Contract.Assert(previousPageStats.Id == currentPageStats.Id);
            return new WikiPageStats(
                currentPageStats.Path, // This path takes precedence over previousPageStats.Path.
                previousPageStats.Id,
                Merge(previousPageStats.DayStats, currentPageStats.DayStats));
        }

        private static WikiPageStats.DayStat[] Merge(
            WikiPageStats.DayStat[] pagePreviousStats,
            WikiPageStats.DayStat[] pageCurrentStats)
        {
            var groupedByDay = pagePreviousStats.Concat(pageCurrentStats).GroupBy(stat => stat.Day);
            var mergedStats = groupedByDay.Select(dayStats =>
            {
                Contract.Assert(dayStats.Count(), "dayStats.Count()", new Range(1, 2),
                    upperBoundReason: "Given day stat may appear once for previous stats and once for current stats");
                Contract.Assert(dayStats.Last().Count >= dayStats.First().Count,
                    "Day stat count for current stats >= day stat count for previous stats");
                return dayStats.Last();
            }).ToArray();
            return mergedStats;
        }

        public static (ValidWikiPagesStats previousMonthStats, ValidWikiPagesStats currentMonthStats) SplitByMonth(
            ValidWikiPagesStats validPagesStats,
            DateTime currentDate)
        {
            var pagesStats = validPagesStats.Value;
            Contract.Assert(pagesStats.Any());

            // For each page, group and order its day stats by month
            var pagesWithOrderedDayStats = pagesStats
                .Select(ps => (ps, dayStatsByMonth: ps.DayStats.GroupAndOrderBy(s => s.Day.Trim(DateTimePrecision.Month))))
                .ToArray();

            // Assert that all days for given month come from that month, for all pages.
            pagesWithOrderedDayStats.ForEach(p =>
                p.dayStatsByMonth.ForEach(ds => ds.items.Assert(dayStat => dayStat.Day.Trim(DateTimePrecision.Month) == ds.key)));

            // Assert there are no duplicate day stats for given (page, month) tuple.
            pagesWithOrderedDayStats.ForEach(p =>
                p.dayStatsByMonth.ForEach(ds => ds.items.AssertDistinctBy(dayStat => dayStat.Day)));

            // For each page, return a tuple of that page stats for previous and current month
            var statsByMonth = pagesWithOrderedDayStats.Select(ps => SplitByMonth(ps, currentDate)).ToArray();

            var previousMonthStats = statsByMonth.Select(t => t.previousMonthPageStats).ToArray();
            var currentMonthStats  = statsByMonth.Select(t => t.currentMonthPageStats).ToArray();
            return (new ValidWikiPagesStats(previousMonthStats), new ValidWikiPagesStats(currentMonthStats));
        }

        private static (WikiPageStats previousMonthPageStats, WikiPageStats currentMonthPageStats)
            SplitByMonth(
                (WikiPageStats stats, (DateTime date, WikiPageStats.DayStat[])[] dayStatsByMonth) page,
                DateTime currentDate)
        {
            Contract.Assert(page.dayStatsByMonth.Length <= 2,
                "The wiki stats are expected to come from no more than 2 months");
            Contract.Assert(page.dayStatsByMonth.Length <= 1 ||
                            (page.dayStatsByMonth[0].date.AddMonths(1) == page.dayStatsByMonth[1].date),
                "The wiki stats are expected to come from consecutive months");

            var previousMonthPageStats = page.stats with
            {
                DayStats = SingleMonthOrderedDayStats(page.dayStatsByMonth, currentDate.AddMonths(-1))
            };
            var currentMonthPageStats = page.stats with
            {
                DayStats = SingleMonthOrderedDayStats(page.dayStatsByMonth, currentDate)
            };
            return (previousMonthPageStats, currentMonthPageStats);

            WikiPageStats.DayStat[] SingleMonthOrderedDayStats(
                (DateTime month, WikiPageStats.DayStat[] dayStatsByMonth)[] dayStatsByDate,
                DateTime date) =>
                dayStatsByDate.Any(stats => stats.month.Month == date.Month)
                    ? dayStatsByDate.Single(stats => stats.month.Month == date.Month).dayStatsByMonth
                        .OrderBy(ds => ds.Day).ToArray()
                    : Array.Empty<WikiPageStats.DayStat>();
        }

        public ValidWikiPagesStats Trim(DateTime startDate, DateTime endDate) => Trim(this, startDate, endDate);

        private static ValidWikiPagesStats Trim(ValidWikiPagesStats stats, DateTime startDate, DateTime endDate) =>
            new(stats.Value.Select(ps =>
                    ps with { DayStats = ps.DayStats.Where(s => s.Day >= startDate && s.Day <= endDate).ToArray() })
                .ToArray());
    }
}