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
//   var persistedStats = new PersistedWikiStats(storageDir);
//   var lastDate = await persistedStats.LastPersistedDate; // need to await as it reads from FileSystem
//   var pageStats = await wiki.PagesStats(DateTime.now - lastDate)
//   var (previousMonthStats, currentMonthStats) = pageStats.SplitByMonth()
//   await new MergedStats(previousMonthStats, persistedStats.previousMonth).Persist();
//   await new MergedStats(currentMonthStats, persistedStats.currentMonth).Persist();
//   // internally, the two above will deserialize the persisted JSON into WikiPageStats,
//   // then merge with ___monthStats WikiPageStats, and then serialize back to the file system.

        public async Task<WikiPagesStatsStorage> Update(AdoWiki wiki, int pageViewsForDays)
        {
            var storedStatsMonths = new MonthlyJsonFilesStore(OS, StorageDirPath);

            var pageStats = await wiki.PagesStats(pageViewsForDays);
            (WikiPageStats[] previousMonthStats, WikiPageStats[] currentMonthStats) = SplitByMonth(pageStats);
            // kja to add
            // var storedcurrentMonth = storedStatsMonths.Read<WikiPageStats[]>(timeline.Now)
            // var storedpreviousMonth = storedStatsMonths.Read<WikiPageStats[]>(timeline.Now.AddMonths(-1))
            // var mergedcurrentMonth = Merge(storedcurrentMonth, currentMonth);
            // var mergedpreviousMonth = Merge(storedpreviousMonth, previousMonth);
            await storedStatsMonths.Write(previousMonthStats, DateTime.UtcNow.AddMonths(-1));
            await storedStatsMonths.Write(currentMonthStats, DateTime.UtcNow);

            // phase 2:
            // storedStatsMonths with {
            //   CurrentMonth = Merged(currentMonthStats, storedStatsMonths.Current),
            //   previousMonth = Merged(previousMonthStats, storedStatsMonths.Last)
            // }
            //

            return this;
        }

        // kja to implement, as follows, in pseudocode:
        // var storedStatsMonths = new MonthlyJsonFilesStore(StorageDirPath)
        // var previousMonthDays = ...; var currentMonthDays = ...
        // return
        //   storedStatsMonth.previousMonth.FilterTo(previousMonthDays)
        //   .Concatenate
        //     storedStatsMonth.CurrentMonth.FilterTo(currentMonthDays)

        public WikiPageStats[] PagesStats(int pageViewsForDays)
        {
            var storedStatsMonths = new MonthlyJsonFilesStore(OS, StorageDirPath);

            var currentMonthDate = Timeline.UtcNow;
            var previousDate     = currentMonthDate.AddDays(-pageViewsForDays + 1);
            var monthsDiffer     = previousDate.Month != currentMonthDate.Month;

            var currentMonthStats = storedStatsMonths.Read<WikiPageStats[]>(currentMonthDate);
            var previousMonthStats = monthsDiffer
                ? storedStatsMonths.Read<WikiPageStats[]>(previousDate)
                : new WikiPageStats[0];

            // kja add filtering here to the pageViewsForDays, i.e. don't use all days of previous month.
            // If necessary, apply this filtering for current month instead of previous month.
            return Merge(previousMonthStats, currentMonthStats);
        }

        // kja test this
        private WikiPageStats[] Merge(WikiPageStats[] previousMonthStats, WikiPageStats[] currentMonthStats)
        {
            var previousStatsByPageId = previousMonthStats.ToDictionary(ps => ps.Id);
            var currentStatsByPageId  = currentMonthStats.ToDictionary(ps => ps.Id);

            var wikiPageStats = previousStatsByPageId.Select(kvp =>
                    currentStatsByPageId.TryGetValue(kvp.Key, out WikiPageStats? currentStats)
                        ? Merge(kvp.Value, currentStats)
                        : kvp.Value)
                .ToArray();

            return wikiPageStats;
        }

        private WikiPageStats Merge(WikiPageStats previousMonthStats, WikiPageStats currentMonthStats)
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
            WikiPageStats[] currentMonthStats = pagesStatsSplitByMonth.Select(t => t.currentMonthPageStats).ToArray();
            return (previousMonthStats, currentMonthStats);
        }

        private static (WikiPageStats previousMonthPageStats, WikiPageStats currentMonthPageStats) ToPageStatsSplitByMonth(
            (WikiPageStats pageStats, ILookup<int, WikiPageStats.Stat> statsByMonth) pageWithStatsGroupedByMonth)
        {
            (int month, WikiPageStats.Stat[])[] statsByMonth =
                pageWithStatsGroupedByMonth.statsByMonth.Select(stats => (stats.Key, stats.ToArray()))
                    // kja this orderBy will likely break on Dec/Jan (12->1)
                    .OrderBy(stats => stats.Key)
                    .ToArray();

            Debug.Assert(statsByMonth.Length <= 2,
                "The wiki stats are expected to come from no more than 2 months");
            // kja This will break in case of December / January. Test for this.
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
// possibly pass it as Options param, like CreateMissingDirs.
// - implement logic as follows, in pseudocode:
//
//   var persistedStats = new PersistedWikiStats(storageDir);
//   var lastDate = await persistedStats.LastPersistedDate; // need to await as it reads from FileSystem
//   var pageStats = await wiki.PagesStats(DateTime.now - lastDate)
//   var (previousMonthStats, currentMonthStats) = pageStats.SplitByMonth()
//   await new MergedStats(previousMonthStats, persistedStats.previousMonth).Persist();
//   await new MergedStats(currentMonthStats, persistedStats.currentMonth).Persist();
//   // internally, the two above will deserialize the persisted JSON into WikiPageStats,
//   // then merge with ___monthStats WikiPageStats, and then serialize back to the file system.
// 
//  - rename everywhere "currentMonth" to "currentMonth".
//
// Later: think about decoupling the logic from FileSystem; maybe arbitrary storage via streams/writers
// would make more sense. At least the merging and splitting algorithm should be decoupled from file system.