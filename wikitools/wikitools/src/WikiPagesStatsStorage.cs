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

            await Storage.With(CurrentDate,               (WikiPageStats[] s) => Merge(s, currentMonthStats));
            await Storage.With(CurrentDate.AddMonths(-1), (WikiPageStats[] s) => Merge(s, previousMonthStats));

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

        private static WikiPageStats.Stat[] Merge(
            WikiPageStats.Stat[] pagePreviousStats,
            WikiPageStats.Stat[] pageCurrentStats)
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

            // For each WikiPageStats, group (key) the stats by month number.
            (WikiPageStats ps, ILookup<DateTime, WikiPageStats.Stat>)[] pagesWithStatsGroupedByDate
                = pagesStats.Select(ps => (ps, ps.Stats.ToLookup(s => s.Day.Trim(DateTimePrecision.Month)))).ToArray();

            // For each page stats, return a tuple of that page stats for last and current month.
            (WikiPageStats previousMonthPageStats, WikiPageStats currentMonthPageStats)[] pagesStatsSplitByMonth =
                pagesWithStatsGroupedByDate.Select(ps => ToPageStatsSplitByMonth(ps, currentDate)).ToArray();

            WikiPageStats[] previousMonthStats = pagesStatsSplitByMonth.Select(t => t.previousMonthPageStats).ToArray();
            WikiPageStats[] currentMonthStats  = pagesStatsSplitByMonth.Select(t => t.currentMonthPageStats).ToArray();
            return (previousMonthStats, currentMonthStats);
        }

        private static (WikiPageStats previousMonthPageStats, WikiPageStats currentMonthPageStats)
            ToPageStatsSplitByMonth(
                (WikiPageStats pageStats, ILookup<DateTime, WikiPageStats.Stat> statsByDate) pageWithStatsGroupedByMonth,
                DateTime currentDate)
        {
            (DateTime date, WikiPageStats.Stat[])[] statsByDate =
                pageWithStatsGroupedByMonth.statsByDate.Select(stats => (stats.Key, stats.ToArray()))
                    .OrderBy(stats => stats.Key)
                    .ToArray();

            Debug.Assert(statsByDate.Length <= 2,
                "The wiki stats are expected to come from no more than 2 months");
            Debug.Assert(statsByDate.Length <= 1 || (statsByDate[0].date.AddMonths(1) == statsByDate[1].date),
                "The wiki stats are expected to come from consecutive months");

            var previousMonthPageStats = pageWithStatsGroupedByMonth.pageStats with
            {
                Stats = SingleMonthStats(statsByDate, currentDate.AddMonths(-1))
            };
            var currentMonthPageStats = pageWithStatsGroupedByMonth.pageStats with
            {
                Stats = SingleMonthStats(statsByDate, currentDate)
            };
            return (previousMonthPageStats, currentMonthPageStats);

            WikiPageStats.Stat[] SingleMonthStats((DateTime date, WikiPageStats.Stat[])[] statsByDate, DateTime date) =>
                statsByDate.Any(sbd => sbd.date.Month == date.Month)
                    ? statsByDate.Single(sbd => sbd.date.Month == date.Month).Item2
                    : new WikiPageStats.Stat[0];
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
