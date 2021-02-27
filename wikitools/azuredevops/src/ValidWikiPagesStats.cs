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
    public class ValidWikiPagesStats : IEnumerable<WikiPageStats>
    {
        // Note this setup of invariant checks in ctor has some problems.
        // Details here: https://github.com/dotnet/csharplang/issues/4453#issuecomment-782807066
        public ValidWikiPagesStats(IEnumerable<WikiPageStats> stats) => Data = CheckInvariants(stats);

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
                Contract.Assert(dayStats.Length >= 0);
                dayStats.AssertDistinctBy(ds => ds.Day);
                dayStats.AssertOrderedBy(ds => ds.Day);
                dayStats.Assert(ds => ds.Count >= 1);
                dayStats.Assert(ds => ds.Day.Kind == DateTimeKind.Utc);
            });
            return statsArr;
        }

        private IEnumerable<WikiPageStats> Data { get; }

        public static ValidWikiPagesStats From(IEnumerable<WikiPageStats> stats) =>
            new(stats.Select(WikiPageStats.FixNulls));

        public (ValidWikiPagesStats previousMonthStats, ValidWikiPagesStats currentMonthStats) SplitByMonth(
            DateTime currentDate) =>
            SplitByMonth(this, currentDate);

        private static (ValidWikiPagesStats previousMonthStats, ValidWikiPagesStats currentMonthStats) SplitByMonth(
            ValidWikiPagesStats pagesStats,
            DateTime currentDate) =>
        (
            new ValidWikiPagesStats(pagesStats.Trim(currentDate.AddMonths(-1))),
            new ValidWikiPagesStats(pagesStats.Trim(currentDate))
        );

        public ValidWikiPagesStats Merge(ValidWikiPagesStats validCurrentStats) => Merge(this, validCurrentStats);

        /// <summary>
        /// Merges ADO Wiki page stats. previousStats with currentStats.
        ///
        /// The merge makes following assumptions about the inputs.
        /// - The arguments obey ValidWikiPagesStats invariants, as evidenced by the argument type.
        /// - However, the union of arguments doesn't necessarily obey these invariants.
        /// Specifically:
        ///   - Page with the same ID might appear twice: once in each argument.
        ///   - Page with the same ID might appear under different paths.
        ///   - A day views stat for a page with given ID and Day date
        ///   can appear twice: once in each argument.
        ///     - In such case, the view count for currentStats is
        ///       greater or equal to the view count of previousStats.
        /// 
        /// The stats returned by Merge obey the following:
        /// - Pages with the same ID are merged into one page.
        ///   - If the Paths were different, the Path from the currentStats is taken.
        ///     The Path from previousStats is discarded.
        ///     - This operations on the assumption the page was renamed,
        ///       and the currentStats are newer, i.e. after the rename.
        /// - A page with given ID has union of all day view stats from both arguments.
        /// - Day view stats for the same Day date for given page are merged
        ///   into one.
        ///
        /// Following invariants are *not* checked, and thus might be
        /// violated by the arguments, even though they are invalid data:
        /// - dates of all day view stats for page with given ID in currentStats
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
                currentPageStats.Path, // This path takes precedence over previousPageStats.Path.
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
                    upperBoundReason: "Given day stat may appear once for previous stats and once for current stats");
                Contract.Assert(dayStats.Last().Count >= dayStats.First().Count,
                    "Day stat count for current stats >= day stat count for previous stats");
                return dayStats.Last();
            }).ToArray();
            return mergedStats;
        }

        public ValidWikiPagesStats Trim(DateTime monthDate) => Trim(
            this,
            monthDate.MonthFirstDay(),
            monthDate.MonthLastDay());

        public ValidWikiPagesStats Trim(DateTime startDate, DateTime endDate) => Trim(this, startDate, endDate);

        private static ValidWikiPagesStats Trim(ValidWikiPagesStats stats, DateTime startDate, DateTime endDate) =>
            new(stats.Select(ps =>
                    ps with { DayStats = ps.DayStats.Where(s => s.Day >= startDate && s.Day <= endDate).ToArray() })
                .ToArray());

        public IEnumerator<WikiPageStats> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}