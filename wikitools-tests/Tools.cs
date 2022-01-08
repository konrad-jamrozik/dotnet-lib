using System;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Config;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
using Xunit;
using Xunit.Abstractions;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.Tests;

public class Tools
{
    private readonly ITestOutputHelper _testOut;

    public Tools(ITestOutputHelper testOut)
    {
        _testOut = testOut;
    }

    [Fact(Skip = "For one-off experiments")]
    public void Scratchpad()
    {
    }

    [Fact(Skip = "Tool to be used manually")]
    public void WriteOutGitRepoClonePaths()
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).ReadFromAssembly<IWikitoolsCfg>();
        var clonePath = cfg.GitRepoClonePath();
        _testOut.WriteLine("Clone path: " + clonePath);
        var filteredPaths = new AdoWikiPagesPaths(fs.FileTree(clonePath).Paths);
        foreach (var fileTreePath in filteredPaths)
        {
            _testOut.WriteLine(fileTreePath);
        }
    }

    [Fact(Skip = "Tool to be used manually")]
    public async Task ToolGetWikiStats()
    {
        ITimeline    timeline = new Timeline();
        IFileSystem  fs       = new FileSystem();
        IEnvironment env      = new Environment();
        IWikitoolsCfg cfg     = new Configuration(fs).ReadFromAssembly<IWikitoolsCfg>();
        IAdoWiki adoWiki = new AdoWiki(
            cfg.AzureDevOpsCfg().AdoWikiUri(),
            cfg.AzureDevOpsCfg().AdoPatEnvVar(),
            env,
            timeline);

        var pagesViewsStats = adoWiki.PagesStats(pageViewsForDays: AdoWiki.PageViewsForDaysMax);

        var storage = new MonthlyJsonFilesStorage(new Dir(fs, cfg.StorageDirPath()));

        await storage.Write(await pagesViewsStats,
            new DateMonth(timeline.UtcNow),
            $"wiki_stats_{timeline.UtcNow:yyyy_MM_dd}_{cfg.AdoWikiPageViewsForDays()}days.json");
    }

    // kj2 I need to ensure this transferring of data scraped from ADO wiki into monthly storage
    // is done during normal program execution, not by this extra tool.
    [Fact(Skip = "Tool to be used manually")]
    public void SplitIntoMonthlyStats()
    {
        IFileSystem fs = new FileSystem();
        var cfg = new Configuration(fs).ReadFromAssembly<IWikitoolsCfg>();
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
                new DateMonth(2021, month));
        }
    }

    private void MergeWikiStatsIntoMonth(
        IFileSystem fs,
        string storageDirPath,
        MonthlyJsonFilesStorage storage,
        DateDay stats1EndDay,
        DateDay stats2EndDay,
        DateMonth outputMonth)
    {
        var stats1Path = storageDirPath + $"/wiki_stats_{stats1EndDay:yyyy_MM_dd}_30days.json";
        var stats1StartDay = stats1EndDay.AddDays(-29);
        var stats1 = DeserializeStats(fs, (stats1Path, stats1StartDay, stats1EndDay));

        var stats2Path = storageDirPath + $"/wiki_stats_{stats2EndDay:yyyy_MM_dd}_30days.json";
        var stats2StartDay = stats2EndDay.AddDays(-29);
        var stats2 = DeserializeStats(fs, (stats2Path, stats2StartDay, stats2EndDay));

        var mergedStats = ValidWikiPagesStats.Merge(new[] { stats1, stats2 });

        storage.Write(
            mergedStats.Trim(outputMonth),
            outputMonth,
            $"date_{outputMonth:yyyy_MM}.json").Wait();
    }

    private static ValidWikiPagesStats DeserializeStats(
        IFileSystem fs,
        (string statsPath, DateDay startDay, DateDay endDay) statsData) =>
        new(
            fs.ReadAllText(statsData.statsPath)
                .FromJsonTo<WikiPageStats[]>(),
            statsData.startDay,
            statsData.endDay);
}