using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record WikiPagesStatsStorage(ITimeline Timeline, IOperatingSystem OS, string StorageDirPath)
    {
        public async Task<WikiPagesStatsStorage> Update(AdoWiki wiki, int pageViewsForDays)
        {
            var storedStatsMonths = new MonthlyJsonFilesStorage(OS, StorageDirPath);

            var pageStats = await wiki.PagesStats(pageViewsForDays);
            (WikiPageStats[] previousMonthStats, WikiPageStats[] currentMonthStats) = SplitByMonth(pageStats);

            var storedCurrentMonthStats  = storedStatsMonths.Read<WikiPageStats[]>(Timeline.UtcNow);
            var storedPreviousMonthStats = storedStatsMonths.Read<WikiPageStats[]>(Timeline.UtcNow.AddMonths(-1));
            
            var mergedCurrentMonthStats  = Merge(storedCurrentMonthStats,  currentMonthStats);
            var mergedPreviousMonthStats = Merge(storedPreviousMonthStats, previousMonthStats);
            
            await storedStatsMonths.Write(mergedCurrentMonthStats,  DateTime.UtcNow);
            await storedStatsMonths.Write(mergedPreviousMonthStats, DateTime.UtcNow.AddMonths(-1));

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

            var currentMonthDate = Timeline.UtcNow;
            var previousDate     = currentMonthDate.AddDays(-pageViewsForDays + 1);
            var monthsDiffer     = previousDate.Month != currentMonthDate.Month;

            var currentMonthStats = storedStatsMonths.Read<WikiPageStats[]>(currentMonthDate);
            var previousMonthStats = monthsDiffer
                ? storedStatsMonths.Read<WikiPageStats[]>(previousDate)
                : new WikiPageStats[0];

            // BUG add filtering here to the pageViewsForDays, i.e. don't use all days of previous month.
            // Note that the following case has to be handled: the *current* (not previous) month needs to be filtered down.
            return Merge(previousMonthStats, currentMonthStats);
        }

        // kja test this
        private static WikiPageStats[] Merge(WikiPageStats[] previousMonthStats, WikiPageStats[] currentMonthStats)
        {
            var currentStatsByPageId  = currentMonthStats.ToDictionary(ps => ps.Id);
            var previousStatsByPageId = previousMonthStats.ToDictionary(ps => ps.Id);

            var currentMonthIds  = currentMonthStats.Select(ps => ps.Id).ToHashSet();
            var previousMonthIds = previousMonthStats.Select(ps => ps.Id).ToHashSet();
            var intersectingIds  = previousMonthIds.Intersect(currentMonthIds);

            var currentMonthOnlyStats  = currentMonthIds.Select(id => currentStatsByPageId[id]);
            var previousMonthOnlyStats = previousMonthIds.Select(id => previousStatsByPageId[id]);
            var intersectingStats =
                intersectingIds.Select(id => Merge(previousStatsByPageId[id], currentStatsByPageId[id]));

            return previousMonthOnlyStats.Union(intersectingStats).Union(currentMonthOnlyStats).ToArray();
        }

        private static WikiPageStats Merge(WikiPageStats previousMonthStats, WikiPageStats currentMonthStats)
        {
            Debug.Assert(previousMonthStats.Path == currentMonthStats.Path);
            Debug.Assert(previousMonthStats.Id == currentMonthStats.Id);
            var path = previousMonthStats.Path;
            var id   = previousMonthStats.Id;
            return new WikiPageStats(path, id, previousMonthStats.Stats.Concat(currentMonthStats.Stats).ToArray());
        }

        public static (WikiPageStats[] previousMonthStats, WikiPageStats[] currentMonthStats) SplitByMonth(
            WikiPageStats[] pagesStats)
        {
            Debug.Assert(pagesStats.Any());

            // For each WikiPageStats, group (key) the stats by month number.
            (WikiPageStats ps, ILookup<int, WikiPageStats.Stat>)[] pagesWithStatsGroupedByMonth
                = pagesStats.Select(ps => (ps, ps.Stats.ToLookup(s => s.Day.Month))).ToArray();

            // For each page stats, return a tuple of that page stats for last and current month.
            (WikiPageStats previousMonthPageStats, WikiPageStats currentMonthPageStats)[] pagesStatsSplitByMonth =
                pagesWithStatsGroupedByMonth.Select(ToPageStatsSplitByMonth).ToArray();

            WikiPageStats[] previousMonthStats = pagesStatsSplitByMonth.Select(t => t.previousMonthPageStats).ToArray();
            WikiPageStats[] currentMonthStats  = pagesStatsSplitByMonth.Select(t => t.currentMonthPageStats).ToArray();
            return (previousMonthStats, currentMonthStats);
        }

        private static (WikiPageStats previousMonthPageStats, WikiPageStats currentMonthPageStats)
            ToPageStatsSplitByMonth(
                (WikiPageStats pageStats, ILookup<int, WikiPageStats.Stat> statsByMonth) pageWithStatsGroupedByMonth)
        {
            (int month, WikiPageStats.Stat[])[] statsByMonth =
                pageWithStatsGroupedByMonth.statsByMonth.Select(stats => (stats.Key, stats.ToArray()))
                    // BUG this orderBy will likely break on Dec/Jan (12->1)
                    .OrderBy(stats => stats.Key)
                    .ToArray();

            Debug.Assert(statsByMonth.Length <= 2,
                "The wiki stats are expected to come from no more than 2 months");
            // BUG This will break in case of December / January. Test for this.
            Debug.Assert(statsByMonth.Length <= 1 || (statsByMonth[0].month + 1 == statsByMonth[1].month),
                "The wiki stats are expected to come from consecutive months");

            return
            (
                pageWithStatsGroupedByMonth.pageStats with
                {
                    Stats = statsByMonth.Length == 2
                        ? statsByMonth.First().Item2
                        : new WikiPageStats.Stat[0]
                },
                // kja add test for statsByMonth.Length == 0, which is when input stats[] length is 0.
                pageWithStatsGroupedByMonth.pageStats with
                {
                    Stats = statsByMonth.Length >= 1
                        ? statsByMonth.Last().Item2
                        : new WikiPageStats.Stat[0]
                }
            );
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