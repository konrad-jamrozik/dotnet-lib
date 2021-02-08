using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
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

            await Storage.With(CurrentDate,               (WikiPageStats[] stats) => Merge(stats, currentMonthStats));
            await Storage.With(CurrentDate.AddMonths(-1), (WikiPageStats[] stats) => Merge(stats, previousMonthStats));

            return this;
        }

        public WikiPageStats[] PagesStats(int pageViewsForDays)
        {
            var currentMonthDate = CurrentDate;
            var previousDate     = currentMonthDate.AddDays(-pageViewsForDays);
            var monthsDiffer     = previousDate.Month != currentMonthDate.Month;

            var currentMonthStats = Storage.Read<WikiPageStats[]>(currentMonthDate);
            var previousMonthStats = monthsDiffer
                ? Storage.Read<WikiPageStats[]>(previousDate)
                : new WikiPageStats[0];

            // BUG (already fixed, needs test) add filtering here to the pageViewsForDays, i.e. don't use all days of previous month.
            // Note that also the following case has to be handled:
            //   the *current* (not previous) month needs to be filtered down.
            return Trim(Merge(previousMonthStats, currentMonthStats), previousDate, CurrentDate);
        }

        // kja test the Merge method
        public static WikiPageStats[] Merge(WikiPageStats[] previousStats, WikiPageStats[] currentStats)
        {
            // kja abstract away this algorithm. Namely, into something like:
            // arr1.IntersectUsing(keySelector: ps => ps.Id, func: (ps1, ps2) => merge(ps1, ps2))

            var previousStatsByPageId = previousStats.ToDictionary(ps => ps.Id);
            var currentStatsByPageId  = currentStats.ToDictionary(ps => ps.Id);

            var previousIds     = Enumerable.ToHashSet(previousStats.Select(ps => ps.Id));
            var currentIds      = Enumerable.ToHashSet(currentStats.Select(ps => ps.Id));
            var intersectingIds = Enumerable.ToHashSet(previousIds.Intersect(currentIds));

            var currentOnlyStats  = currentIds.Except(intersectingIds).Select(id => currentStatsByPageId[id]);
            var previousOnlyStats = previousIds.Except(intersectingIds).Select(id => previousStatsByPageId[id]);
            var intersectingStats =
                intersectingIds.Select(id => Merge(previousStatsByPageId[id], currentStatsByPageId[id]));

            var merged = previousOnlyStats.Union(intersectingStats).Union(currentOnlyStats).ToArray();

            Debug.Assert(merged.DistinctBy(m => m.Id).Count() == merged.Length, "Any given page appears only once");
            merged.ForEach(ps => Debug.Assert(
                ps.Stats.DistinctBy(s => s.Day).Count() == ps.Stats.Length,
                "There is only one stat per page per day"));

            return merged;
        }

        private static WikiPageStats Merge(WikiPageStats previousPageStats, WikiPageStats currentPageStats)
        {
            Debug.Assert(previousPageStats.Id == currentPageStats.Id);
            var id = previousPageStats.Id;

            var previousMaxDate = previousPageStats.Stats.Any()
                ? previousPageStats.Stats.Max(stat => stat.Day)
                : new DateTime(0);
            var currentMaxDate = currentPageStats.Stats.Any()
                ? currentPageStats.Stats.Max(stat => stat.Day)
                : new DateTime(0);
            // Here we ensure that the 'current' stats path takes precedence over 'previous' page stats.
            var path = previousMaxDate > currentMaxDate ? previousPageStats.Path : currentPageStats.Path;

            return new WikiPageStats(path, id, Merge(previousPageStats.Stats, currentPageStats.Stats));
        }

        private static WikiPageStats.DayStat[] Merge(
            WikiPageStats.DayStat[] pagePreviousStats,
            WikiPageStats.DayStat[] pageCurrentStats)
        {
            var groupedByDay = pagePreviousStats.Concat(pageCurrentStats).GroupBy(stat => stat.Day);
            var mergedStats = groupedByDay.Select(dayStats =>
            {
                Debug.Assert(dayStats.DistinctBy(s => s.Count).Count() == 1,
                    "Stats merged for the same day have the same visit count");
                return dayStats.First();
            }).ToArray();
            return mergedStats;
        }

        public static (WikiPageStats[] previousMonthStats, WikiPageStats[] currentMonthStats) SplitByMonth(
            WikiPageStats[] pagesStats,
            DateTime currentDate)
        {
            Debug.Assert(pagesStats.Any());

            // For each page, group and order its day stats by month
            var pagesWithOrderedDayStats = pagesStats
                .Select(ps => (ps, ps.Stats.GroupAndOrderBy(s => s.Day.Trim(DateTimePrecision.Month)))).ToArray();

            // For each page, return a tuple of that page stats for previous and current month
            var pagesStatsSplitByMonth = pagesWithOrderedDayStats.Select(ps => SplitByMonth(ps, currentDate)).ToArray();

            WikiPageStats[] previousMonthStats = pagesStatsSplitByMonth.Select(t => t.previousMonthPageStats).ToArray();
            WikiPageStats[] currentMonthStats  = pagesStatsSplitByMonth.Select(t => t.currentMonthPageStats).ToArray();
            return (previousMonthStats, currentMonthStats);
        }

        private static (WikiPageStats previousMonthPageStats, WikiPageStats currentMonthPageStats)
            SplitByMonth(
                (WikiPageStats stats, (DateTime date, WikiPageStats.DayStat[])[] dayStatsByDate) page,
                DateTime currentDate)
        {
            Debug.Assert(page.dayStatsByDate.Length <= 2,
                "The wiki stats are expected to come from no more than 2 months");
            Debug.Assert(page.dayStatsByDate.Length <= 1 ||
                         (page.dayStatsByDate[0].date.AddMonths(1) == page.dayStatsByDate[1].date),
                "The wiki stats are expected to come from consecutive months");

            var previousMonthPageStats = page.stats with
            {
                Stats = SingleMonthStats(page.dayStatsByDate, currentDate.AddMonths(-1))
            };
            var currentMonthPageStats = page.stats with
            {
                Stats = SingleMonthStats(page.dayStatsByDate, currentDate)
            };
            return (previousMonthPageStats, currentMonthPageStats);

            WikiPageStats.DayStat[] SingleMonthStats(
                (DateTime date, WikiPageStats.DayStat[] dayStatsByDate)[] dayStatsByDateByDate,
                DateTime date) =>
                dayStatsByDateByDate.Any(statsTuple => statsTuple.date.Month == date.Month)
                    ? dayStatsByDateByDate.Single(statsTuple => statsTuple.date.Month == date.Month).dayStatsByDate
                    : new WikiPageStats.DayStat[0];
        }

        public static WikiPageStats[] Trim(WikiPageStats[] stats, DateTime startDate, DateTime endDate) =>
            stats.Select(ps =>
                ps with { Stats = ps.Stats.Where(s => s.Day >= startDate && s.Day <= endDate).ToArray() }).ToArray();
    }
}

// kja curr work.
// Next tasks in Wikitools.WikiPagesStatsStorage.Update
// - deduplicate serialization logic with JsonDiff
// - replace File.WriteAllTextAsync. Introduce File abstraction or similar,
// that depends on OS.FileSystem. Make it create Dirs as needed when writing out.
//
// Later: think about decoupling the logic from FileSystem; maybe arbitrary storage via streams/writers
// would make more sense. At least the merging and splitting algorithm should be decoupled from file system.