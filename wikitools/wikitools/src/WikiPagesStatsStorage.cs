using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public record WikiPagesStatsStorage(IOperatingSystem OS, string StorageDirPath)
    {
//   var persistedStats = new PersistedWikiStats(storageDir);
//   var lastDate = await persistedStats.LastPersistedDate; // need to await as it reads from FileSystem
//   var pageStats = await wiki.PagesStats(DateTime.now - lastDate)
//   var (lastMonthStats, thisMonthStats) = pageStats.SplitByMonth()
//   await new MergedStats(lastMonthStats, persistedStats.LastMonth).Persist();
//   await new MergedStats(thisMonthStats, persistedStats.ThisMonth).Persist();
//   // internally, the two above will deserialize the persisted JSON into WikiPageStats,
//   // then merge with ___monthStats WikiPageStats, and then serialize back to the file system.

        public async Task<WikiPagesStatsStorage> Update(AdoWiki wiki, int pageViewsForDays)
        {
            var storedStatsMonths = new MonthlyJsonFilesStore(OS, StorageDirPath);

            var pageStats = await wiki.PagesStats(pageViewsForDays);
            (WikiPageStats[] lastMonthStats, WikiPageStats[] thisMonthStats) = SplitByMonth(pageStats);
            // kja to add
            // var storedThisMonth = storedStatsMonths.Read<WikiPageStats[]>(timeline.Now)
            // var storedLastMonth = storedStatsMonths.Read<WikiPageStats[]>(timeline.Now.AddMonths(-1))
            // var mergedThisMonth = Merge(storedThisMonth, thisMonth);
            // var mergedLastMonth = Merge(storedLastMonth, lastMonth);
            await storedStatsMonths.Write(lastMonthStats, DateTime.UtcNow.AddMonths(-1));
            await storedStatsMonths.Write(thisMonthStats, DateTime.UtcNow);

            // phase 2:
            // storedStatsMonths with {
            //   CurrentMonth = Merged(thisMonthStats, storedStatsMonths.Current),
            //   LastMonth = Merged(lastMonthStats, storedStatsMonths.Last)
            // }
            //

            return this;
        }

        // kja to implement, as follows, in pseudocode:
        // var storedStatsMonths = new MonthlyJsonFilesStore(StorageDirPath)
        // var lastMonthDays = ...; var thisMonthDays = ...
        // return
        //   storedStatsMonth.LastMonth.FilterTo(lastMonthDays)
        //   .Concatenate
        //     storedStatsMonth.CurrentMonth.FilterTo(currentMonthDays)

        public Task<WikiPageStats[]> PagesStats(int pageViewsForDays)
        {
            var storedStatsMonths = new MonthlyJsonFilesStore(OS, StorageDirPath);

            var maxDate = storedStatsMonths.FindMaxDate();
            var stats   = storedStatsMonths.Read<WikiPageStats[]>(maxDate);
            // kja to add: find second-last date (if necessary), read json, merge
            return Task.FromResult(stats);
        }

        public static (WikiPageStats[] lastMonthStats, WikiPageStats[] thisMonthStats) SplitByMonth(
            WikiPageStats[] pagesStats)
        {
            Debug.Assert(pagesStats.Any());

            // For each WikiPageStats, group the stats by month number.
            (WikiPageStats ps, ILookup<int, WikiPageStats.Stat>)[] pagesWithStatsGroupedByMonth
                = pagesStats.Select(ps => (ps, ps.Stats.ToLookup(s => s.Day.Month))).ToArray();

            // For each page stats, return a tuple of that page stats for last and current month.
            (WikiPageStats lastMonthPageStats, WikiPageStats thisMonthPageStats)[] pagesStatsSplitByMonth =
                pagesWithStatsGroupedByMonth.Select(ToPageStatsSplitByMonth).ToArray();

            WikiPageStats[] lastMonthStats = pagesStatsSplitByMonth.Select(t => t.lastMonthPageStats).ToArray();
            WikiPageStats[] thisMonthStats = pagesStatsSplitByMonth.Select(t => t.thisMonthPageStats).ToArray();
            return (lastMonthStats, thisMonthStats);
        }

        private static (WikiPageStats lastMonthPageStats, WikiPageStats thisMonthPageStats) ToPageStatsSplitByMonth(
            (WikiPageStats pageStats, ILookup<int, WikiPageStats.Stat> statsByMonth) pageWithStatsGroupedByMonth)
        {
            (int month, WikiPageStats.Stat[])[] statsByMonth =
                pageWithStatsGroupedByMonth.statsByMonth.Select(stats => (stats.Key, stats.ToArray()))
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
//   var (lastMonthStats, thisMonthStats) = pageStats.SplitByMonth()
//   await new MergedStats(lastMonthStats, persistedStats.LastMonth).Persist();
//   await new MergedStats(thisMonthStats, persistedStats.ThisMonth).Persist();
//   // internally, the two above will deserialize the persisted JSON into WikiPageStats,
//   // then merge with ___monthStats WikiPageStats, and then serialize back to the file system.
// 
//  - rename everywhere "thisMonth" to "currentMonth".
//
// Later: think about decoupling the logic from FileSystem; maybe arbitrary storage via streams/writers
// would make more sense. At least the merging and splitting algorithm should be decoupled from file system.