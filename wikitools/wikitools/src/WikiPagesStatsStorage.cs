using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public record WikiPagesStatsStorage(DateTime CurrentDate, IOperatingSystem OS, string StorageDirPath)
    {
        public async Task<WikiPagesStatsStorage> Update(AdoWiki wiki, int pageViewsForDays)
        {
            var storedStats = new MonthlyJsonFilesStorage(OS, StorageDirPath);

            var pageStats = await wiki.PagesStats(pageViewsForDays);
            (WikiPageStats[] previousMonthStats, WikiPageStats[] currentMonthStats) =
                SplitByMonth(pageStats, CurrentDate);

            var storedPreviousMonthStats = storedStats.Read<WikiPageStats[]>(CurrentDate.AddMonths(-1));
            var storedCurrentMonthStats  = storedStats.Read<WikiPageStats[]>(CurrentDate);

            var mergedPreviousMonthStats = Merge(storedPreviousMonthStats, previousMonthStats);
            var mergedCurrentMonthStats  = Merge(storedCurrentMonthStats,  currentMonthStats);

            await storedStats.Write(mergedPreviousMonthStats, DateTime.UtcNow.AddMonths(-1));
            await storedStats.Write(mergedCurrentMonthStats,  DateTime.UtcNow);

            // kja impl phase 2:
            // storedStatsMonths with {
            //   CurrentMonth = Merged(currentMonthStats, storedStatsMonths.Current),
            //   previousMonth = Merged(previousMonthStats, storedStatsMonths.Last)
            // }
            //

            return this;
        }

        public WikiPageStats[] PagesStats(int pageViewsForDays)
        {
            var storedStatsMonths = new MonthlyJsonFilesStorage(OS, StorageDirPath);

            var currentMonthDate = CurrentDate;
            var previousDate     = currentMonthDate.AddDays(-pageViewsForDays + 1);
            var monthsDiffer     = previousDate.Month != currentMonthDate.Month;

            var currentMonthStats = storedStatsMonths.Read<WikiPageStats[]>(currentMonthDate);
            var previousMonthStats = monthsDiffer
                ? storedStatsMonths.Read<WikiPageStats[]>(previousDate)
                : new WikiPageStats[0];

            // BUG add filtering here to the pageViewsForDays, i.e. don't use all days of previous month.
            // Note that also the following case has to be handled:
            //   the *current* (not previous) month needs to be filtered down.
            // test for this.
            return Merge(previousMonthStats, currentMonthStats);
        }

        // kja test the Merge method
        // kja add assertions that the merged stats can come from max 2 months, then test for this.
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
                Debug.Assert(dayStats.DistinctBy(s => s.Count).Count() == 1);
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
            (WikiPageStats ps, ILookup<int, WikiPageStats.Stat>)[] pagesWithStatsGroupedByMonth
                = pagesStats.Select(ps => (ps, ps.Stats.ToLookup(s => s.Day.Month))).ToArray();

            // For each page stats, return a tuple of that page stats for last and current month.
            (WikiPageStats previousMonthPageStats, WikiPageStats currentMonthPageStats)[] pagesStatsSplitByMonth =
                pagesWithStatsGroupedByMonth.Select(pt => ToPageStatsSplitByMonth(pt, currentDate)).ToArray();

            WikiPageStats[] previousMonthStats = pagesStatsSplitByMonth.Select(t => t.previousMonthPageStats).ToArray();
            WikiPageStats[] currentMonthStats  = pagesStatsSplitByMonth.Select(t => t.currentMonthPageStats).ToArray();
            return (previousMonthStats, currentMonthStats);
        }

        private static (WikiPageStats previousMonthPageStats, WikiPageStats currentMonthPageStats)
            ToPageStatsSplitByMonth(
                (WikiPageStats pageStats, ILookup<int, WikiPageStats.Stat> statsByMonth) pageWithStatsGroupedByMonth,
                DateTime currentDate)
        {
            (int month, WikiPageStats.Stat[])[] statsByMonth =
                pageWithStatsGroupedByMonth.statsByMonth.Select(stats => (stats.Key, stats.ToArray()))
                    // BUG this orderBy will break on Dec/Jan (12->1)
                    .OrderBy(stats => stats.Key)
                    .ToArray();

            Debug.Assert(statsByMonth.Length <= 2,
                "The wiki stats are expected to come from no more than 2 months");
            // BUG This will break in case of December / January. Test for this.
            Debug.Assert(statsByMonth.Length <= 1 || (statsByMonth[0].month + 1 == statsByMonth[1].month),
                "The wiki stats are expected to come from consecutive months");

            var previousMonthPageStats = pageWithStatsGroupedByMonth.pageStats with { Stats = MonthStats(statsByMonth, currentDate.AddMonths(-1)) };
            // BUG (maybe already fixed?? not sure) add test for statsByMonth.Length == 0, which is when input stats[] length is 0.
            var currentMonthPageStats = pageWithStatsGroupedByMonth.pageStats with { Stats = MonthStats(statsByMonth, currentDate) };
            return (previousMonthPageStats, currentMonthPageStats);

            WikiPageStats.Stat[] MonthStats((int month, WikiPageStats.Stat[])[] statsByMonth, DateTime date) =>
                statsByMonth.Any(sbm => sbm.month == date.Month)
                    ? statsByMonth.Single(sbm => sbm.month == date.Month).Item2
                    : new WikiPageStats.Stat[0];
        }
    }
}

// kja  curr work.
// Next tasks in Wikitools.WikiPagesStatsStorage.Update
// - deduplicate serialization logic with JsonDiff
// - replace File.WriteAllTextAsync. Introduce File abstraction or similar,
// that depends on OS.FileSystem. Make it create Dirs as needed when writing out.
//
// Later: think about decoupling the logic from FileSystem; maybe arbitrary storage via streams/writers
// would make more sense. At least the merging and splitting algorithm should be decoupled from file system.
// kja merge in the backed up stats for January