using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Config;
using Wikitools.Lib.Configuration;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
using Wikitools.Lib.Tests.Json;
using Xunit;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.Tests;

public class AdoWikiStatsTools
{
    [Fact(Skip = "Tool to be used manually")]
    public async Task ToolGetWikiStats()
    {
        ITimeline     timeline = new Timeline();
        IFileSystem   fs       = new FileSystem();
        IEnvironment  env      = new Environment();
        IWikitoolsCfg cfg      = new Configuration(fs).Load<IWikitoolsCfg>();
        IAdoWiki wiki = new AdoWiki(
            cfg.AzureDevOpsCfg().AdoWikiUri(),
            cfg.AzureDevOpsCfg().AdoPatEnvVar(),
            env);

        var pagesViewsStats = wiki.PagesStats(PageViewsForDays.Max);

        var storageDir = StorageDirWithStatsFiles(fs, cfg);
        var storage = new MonthlyJsonFilesStorage(storageDir);
        var statsFile = new WikiStatsFile(
            storageDir,
            timeline.UtcNow,
            cfg.AdoWikiPageViewsForDays());

        await storage.Write(await pagesViewsStats,
            new DateMonth(timeline.UtcNow),
            statsFile.Name);
    }

    [Fact]
    public void ToolTransferStatsFilesIntoMonthlyStorage()
    {
        IFileSystem   fs  = new FileSystem();
        IWikitoolsCfg cfg = new Configuration(fs).Load<IWikitoolsCfg>();
        Dir storageDir = StorageDirWithStatsFiles(fs, cfg);

        // 1. Obtain references to all files in storage directory
        // containing the saved raw stats from ADO wiki
        List<WikiStatsFile> statsFiles = storageDir
            .GetFiles(filterRegexPattern: WikiStatsFile.Regex)
            .Select(file => new WikiStatsFile(file))
            .ToList();

        // 2. Deserialize stats from these files.
        List<ValidWikiPagesStats> stats = statsFiles
            .Select(statsFile => statsFile.Stats)
            .ToList();

        // 3. Partition these stats by month
        ValidWikiPagesStats mergedStats = ValidWikiPagesStats.Merge(stats);
        // kja implement PartitionByMonth
        List<ValidWikiPagesStatsForMonth> statsByMonths = mergedStats.PartitionByMonth();

        // 4. Verify which stats from ADO wiki correspond with the previously stored stats
        var storage = new MonthlyJsonFilesStorage(storageDir);
        var allDiffsEmpty = statsByMonths.Select(
            monthStats =>
            {
                IEnumerable<WikiPageStats> rawStoredStatsForMonth =
                    storage.Read<IEnumerable<WikiPageStats>>(monthStats.Month);
                var storedStatsForMonth = new ValidWikiPagesStatsForMonth(
                    rawStoredStatsForMonth,
                    monthStats.Month);
                var diff = new JsonDiff(storedStatsForMonth, monthStats);
                new JsonDiffAssertion(diff).Assert();
                return diff.IsEmpty;
            }).All(diffIsEmpty => diffIsEmpty);

        // 5. Upsert the stats from ADO wiki into the storage
        Task[] writeTasks = statsByMonths
            .Take(0) // kja Take(0) until I get the diff and dry-run flag running
            .Select(monthStats => storage.Write(monthStats, monthStats.Month))
            .ToArray();
        Task.WaitAll(writeTasks);

        
        // DONE 1. obtain references to all wiki_stats_ files in given directory
        //
        //    // This needs to parse not only the file contents, but also data from file name: date and int.
        //    var statsFiles = GetAllFilesInDir(dir: cfg.StorageDirPath() filePattern: regex for wiki_stats_[yyyy_MM_dd]_[int]days.json")
        //
        // DONE 2. deserialize stats from these files
        //
        //    List<ValidWikiPagesStats> stats = statsFiles.Select(file => DeserializeStats(file, GetDaySpan(file.date, file.int)))
        //    
        // DONE 3. merge the stats into one huge valid wiki stats
        //
        //    ValidWikiPagesStats mergedStats = ValidWikiPagesStats.Merge(stats);
        //
        // 4. partition them by month, in preparation for each month to be
        // saved to corresponding monthly stats file
        //
        //    // See also https://morelinq.github.io/2.4/ref/api/html/M_MoreLinq_MoreEnumerable_Partition__2_2.htm
        //    List<ValidWikiPagesStats> statsByMonth = mergedStats.PartitionByMonth()
        //
        // 5. for each month, compare the stats-about-to-be-saved with the
        // the already saved monthly stats, if any.
        // 5.1. If they differ, then report this to stdout. If dryRun=false, override.
        //
        //    var statsDiffs = statsByMonth.ForEach(stats => new JsonDiff(monthlyStatsStorage.Read(stats.Month), stats))
        //    var nonemptyDiffs = statsDiffs.Where(diff => diff.IsNonempty)
        //    nonemptyDiffs.ForEach(diff => LogToStdout(diff);
        //    if (dryRun == false)
        //      // Note that here actually NewStatsForTheDiff method is bogus; instead, when computing the diffs
        //      // in the first place, they need to stay correlated with the stats, e.g. in a dictionary.
        //      nonemptyDiffs.ForEach(diff => storage.Write(NewStatsForTheDiff(diff)))
        //
        // ------------------------------------------
        // Alternative mode: when ToolGetWikiStats runs, execute this logic,
        // but only for the most recent two months (up to thePageViewsForDays.Max)
        //
        // Note:
        // The transfer is from files like wiki_stats_2022_07_02_30days.json
        // to files like date_2022_07.json
        // (format from Wikitools.Lib.Storage.MonthlyJsonFilesStorage.FileName)
    }

    [Fact(Skip = "Tool to be used manually")]
    public void SplitIntoMonthlyStats()
    {
        IFileSystem fs = new FileSystem();
        var cfg = new Configuration(fs).Load<IWikitoolsCfg>();
        var storage = new MonthlyJsonFilesStorage(new Dir(fs, cfg.StorageDirPath()));

        var data = new (int month, (int month, int day) stats1, (int month, int day) stats2)[]
        {
            (month: 1, (month: 1, day: 19), (month: 2, day: 7)),
            (month: 2, (month: 2, day: 7), (month: 3, day: 3)),
            (month: 3, (month: 3, day: 28), (month: 4, day: 26)),
            (month: 4, (month: 4, day: 26), (month: 5, day: 4)),
            (month: 5, (month: 5, day: 14), (month: 6, day: 4)),
            (month: 6, (month: 6, day: 19), (month: 7, day: 15)),
            (month: 7, (month: 7, day: 15), (month: 8, day: 3)),
            (month: 8, (month: 8, day: 13), (month: 9, day: 6)),
            (month: 9, (month: 9, day: 24), (month: 10, day: 23)),
            (month: 10, (month: 10, day: 23), (month: 11, day: 14)),
            (month: 11, (month: 11, day: 14), (month: 11, day: 28))
        };

        foreach (var (month, stats1, stats2) in data)
        {
            MergeWikiStatsIntoMonth(
                fs,
                cfg.StorageDirPath(),
                storage,
                new DateDay(2021, stats1.month, stats1.day, DateTimeKind.Utc),
                new DateDay(2021, stats2.month, stats2.day, DateTimeKind.Utc),
                new DateMonth(2021, month, DateTimeKind.Utc));
        }
    }

    private static Dir StorageDirWithStatsFiles(IFileSystem fs, IWikitoolsCfg cfg)
        => new Dir(fs, cfg.StorageDirPath());

    private void MergeWikiStatsIntoMonth(
        IFileSystem fs,
        string storageDirPath,
        MonthlyJsonFilesStorage storage,
        DateDay stats1EndDay,
        DateDay stats2EndDay,
        DateMonth outputMonth)
    {
        // kja commented out because I moved DeserializeStats to WikiStatsFile

        // var stats1Path = storageDirPath + $"/wiki_stats_{stats1EndDay:yyyy_MM_dd}_30days.json";
        // var stats1StartDay = stats1EndDay.AddDays(-29);
        // var stats1 = DeserializeStats(fs, (stats1Path, new DaySpan(stats1StartDay, stats1EndDay)));
        //
        // var stats2Path = storageDirPath + $"/wiki_stats_{stats2EndDay:yyyy_MM_dd}_30days.json";
        // var stats2StartDay = stats2EndDay.AddDays(-29);
        // var stats2 = DeserializeStats(fs, (stats2Path, new DaySpan(stats2StartDay, stats2EndDay)));

        //var mergedStats = ValidWikiPagesStats.Merge(new[] { stats1, stats2 });

        //storage.Write(
        //    mergedStats.Trim(outputMonth),
        //    outputMonth,
        //    // kja this duplicates Wikitools.Lib.Storage.MonthlyJsonFilesStorage.FileName
        //    $"date_{outputMonth:yyyy_MM}.json").Wait();
    }
}