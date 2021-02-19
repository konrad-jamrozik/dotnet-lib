using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record WikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateTime CurrentDate)
    {
        public async Task<WikiPagesStatsStorage> Update(AdoWiki wiki, int pageViewsForDays)
        {
            var pageStats = await wiki.PagesStats(pageViewsForDays);

            var (previousMonthStats, currentMonthStats) = SplitByMonth(pageStats, CurrentDate);

            await Storage.With(CurrentDate,               (WikiPageStats[] stats) => 
                Merge(new ValidWikiPagesStats(stats.Select(WikiPageStats.FixNulls)), currentMonthStats).Value);
            await Storage.With(CurrentDate.AddMonths(-1), (WikiPageStats[] stats) => 
                Merge(new ValidWikiPagesStats(stats.Select(WikiPageStats.FixNulls)), previousMonthStats).Value);

            return this;
        }

        public ValidWikiPagesStats PagesStats(int pageViewsForDays)
        {
            var currentMonthDate = CurrentDate;
            var previousDate     = currentMonthDate.AddDays(-pageViewsForDays);
            var monthsDiffer     = previousDate.Month != currentMonthDate.Month;

            var currentMonthStats = new ValidWikiPagesStats(Storage.Read<WikiPageStats[]>(currentMonthDate));
            var previousMonthStats = new ValidWikiPagesStats(monthsDiffer
                ? Storage.Read<WikiPageStats[]>(previousDate)
                : new WikiPageStats[0]);

            // BUG 2 (already fixed, needs test) add filtering here to the pageViewsForDays, i.e. don't use all days of previous month.
            // Note that also the following case has to be tested for:
            //   the *current* (not previous) month needs to be filtered down.
            return new ValidWikiPagesStats(Trim(Merge(previousMonthStats, currentMonthStats).Value, previousDate, CurrentDate));
        }

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
        ///   can appear in appear twice: once in each argument.
        ///     - In such case, the view counts for that day
        ///     are guaranteed to be the same.
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
        public static ValidWikiPagesStats Merge(ValidWikiPagesStats validPreviousStats, ValidWikiPagesStats validCurrentStats)
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

            Debug.Assert(merged.DistinctBy(m => m.Id).Count() == merged.Length, "Any given page appears only once");
            merged.ForEach(ps => Debug.Assert(
                ps.DayStats.DistinctBy(s => s.Day).Count() == ps.DayStats.Length,
                "There is only one stat per page per day"));

            return new ValidWikiPagesStats(merged);
        }

        private static WikiPageStats Merge(WikiPageStats previousPageStats, WikiPageStats currentPageStats)
        {
            Debug.Assert(previousPageStats.Id == currentPageStats.Id);
            var id = previousPageStats.Id;

            var previousMaxDate = previousPageStats.DayStats.Any()
                ? previousPageStats.DayStats.Max(stat => stat.Day)
                : new DateTime(0);
            var currentMaxDate = currentPageStats.DayStats.Any()
                ? currentPageStats.DayStats.Max(stat => stat.Day)
                : new DateTime(0);
            // kja 1 test for proper page renames in Merge
            // Here we ensure that the 'current' stats path takes precedence over 'previous' page stats.
            var path = previousMaxDate > currentMaxDate ? previousPageStats.Path : currentPageStats.Path;

            return new WikiPageStats(path, id, Merge(previousPageStats.DayStats, currentPageStats.DayStats));
        }

        private static WikiPageStats.DayStat[] Merge(
            WikiPageStats.DayStat[] pagePreviousStats,
            WikiPageStats.DayStat[] pageCurrentStats)
        {
            var groupedByDay = pagePreviousStats.Concat(pageCurrentStats).GroupBy(stat => stat.Day);
            var mergedStats = groupedByDay.Select(dayStats =>
            {
                dayStats.AssertSingleBy(s => s.Count, "DayStats merged for the same day have the same visit count");
                return dayStats.First();
            }).ToArray();
            return mergedStats;
        }

        public static (ValidWikiPagesStats previousMonthStats, ValidWikiPagesStats currentMonthStats) SplitByMonth(
            ValidWikiPagesStats validPagesStats,
            DateTime currentDate)
        {
            var pagesStats = validPagesStats.Value;
            Debug.Assert(pagesStats.Any());

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
            Debug.Assert(page.dayStatsByMonth.Length <= 2,
                "The wiki stats are expected to come from no more than 2 months");
            Debug.Assert(page.dayStatsByMonth.Length <= 1 ||
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

        // kja make it a method on valid stats; probably same with Merge and Split
        public static WikiPageStats[] Trim(WikiPageStats[] stats, DateTime startDate, DateTime endDate) =>
            stats.Select(ps =>
                ps with { DayStats = ps.DayStats.Where(s => s.Day >= startDate && s.Day <= endDate).ToArray() }).ToArray();
    }
}

// kja 4 high level next tasks
// Next tasks in Wikitools.WikiPagesStatsStorage.Update
// - deduplicate serialization logic with JsonDiff
// - replace File.WriteAllTextAsync. Introduce File abstraction or similar,
// that depends on OS.FileSystem. Make it create Dirs as needed when writing out.
//
// Later: think about decoupling the logic from FileSystem; maybe arbitrary storage via streams/writers
// would make more sense. At least the merging and splitting algorithm should be decoupled from file system.