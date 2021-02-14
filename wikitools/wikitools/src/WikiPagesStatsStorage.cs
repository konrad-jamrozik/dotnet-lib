﻿using System;
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

        public static WikiPageStats[] Merge(WikiPageStats[] previousStats, WikiPageStats[] currentStats)
        {
            var previousStatsByPageId = previousStats.ToDictionary(ps => ps.Id);
            var currentStatsByPageId  = currentStats.ToDictionary(ps => ps.Id);

            var previousIds     = previousStats.Select(ps => ps.Id).ToHashSet();
            var currentIds      = currentStats.Select(ps => ps.Id).ToHashSet();
            var intersectingIds = previousIds.Intersect(currentIds).ToHashSet();

            var currentOnlyStats  = currentIds.Except(intersectingIds).Select(id => currentStatsByPageId[id]);
            var previousOnlyStats = previousIds.Except(intersectingIds).Select(id => previousStatsByPageId[id]);
            var intersectingStats =
                intersectingIds.Select(id => Merge(previousStatsByPageId[id], currentStatsByPageId[id]));

            var merged = previousOnlyStats.Union(intersectingStats).Union(currentOnlyStats).ToArray();
            // kja once tested, replace with:
            var merged2 = previousStats.UnionUsing(currentStats, ps => ps.Id, Merge);

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
                .Select(ps => (ps, dayStatsByMonth: ps.Stats.GroupAndOrderBy(s => s.Day.Trim(DateTimePrecision.Month))))
                .ToArray();

            // Assert there are no duplicate day stats for given (page, month) tuple.
            pagesWithOrderedDayStats.ForEach(p =>
                p.dayStatsByMonth.ForEach(ds => ds.items.AssertDistinctBy(dayStat => dayStat.Day)));

            // For each page, return a tuple of that page stats for previous and current month
            var statsByMonth = pagesWithOrderedDayStats.Select(ps => SplitByMonth(ps, currentDate)).ToArray();

            var previousMonthStats = statsByMonth.Select(t => t.previousMonthPageStats).ToArray();
            var currentMonthStats  = statsByMonth.Select(t => t.currentMonthPageStats).ToArray();
            return (previousMonthStats, currentMonthStats);
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
                Stats = SingleMonthStats(page.dayStatsByMonth, currentDate.AddMonths(-1))
            };
            var currentMonthPageStats = page.stats with
            {
                Stats = SingleMonthStats(page.dayStatsByMonth, currentDate)
            };
            return (previousMonthPageStats, currentMonthPageStats);

            WikiPageStats.DayStat[] SingleMonthStats(
                (DateTime month, WikiPageStats.DayStat[] dayStatsByMonth)[] dayStatsByDate,
                DateTime date) =>
                dayStatsByDate.Any(stats => stats.month.Month == date.Month)
                    ? dayStatsByDate.Single(stats => stats.month.Month == date.Month).dayStatsByMonth
                    : Array.Empty<WikiPageStats.DayStat>();
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