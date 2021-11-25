using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
using Xunit;
using Xunit.Abstractions;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.Tests
{
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
            WikitoolsCfg cfg = new Configuration(fs).Read<WikitoolsCfg>();
            var clonePath = cfg.GitRepoClonePath;
            _testOut.WriteLine("Clone path: " + clonePath);
            var filteredPaths = new AdoWikiPagesPaths(fs.FileTree(clonePath).Paths);
            foreach (var fileTreePath in filteredPaths)
            {
                _testOut.WriteLine(fileTreePath);
            }
        }

        [Fact(Skip = "Scratchpad test")]
        public void TypeReflectionScratchpad()
        {
            foreach (var info in typeof(WikitoolsCfg).GetProperties())
            {
                if (info.Name.EndsWith(IConfiguration.ConfigSuffix))
                {
                    _testOut.WriteLine(
                        $"{info.Name} {info.MemberType} {info.PropertyType} {IConfiguration.FileName(info.PropertyType)}");
                }
            }
        }

        // kja apparently there is hole in the stored monthly stats for Sep-Oct as I didn't run this often enough.
        // I need to ensure that normal program execution fixes it, or at least detects
        [Fact(Skip = "Tool to be used manually")]
        public async Task ToolGetWikiStats()
        {
            ITimeline    timeline = new Timeline();
            IFileSystem  fs       = new FileSystem();
            IEnvironment env      = new Environment();
            WikitoolsCfg cfg = new Configuration(fs).Read<WikitoolsCfg>();
            IAdoWiki adoWiki = new AdoWiki(
                cfg.AzureDevOpsCfg.AdoWikiUri,
                cfg.AzureDevOpsCfg.AdoPatEnvVar,
                env,
                timeline);

            var pagesViewsStats = adoWiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            var storage = new MonthlyJsonFilesStorage(new Dir(fs, cfg.StorageDirPath));

            await storage.Write(await pagesViewsStats,
                new DateMonth(timeline.UtcNow),
                $"wiki_stats_{timeline.UtcNow:yyyy_MM_dd}_{cfg.AdoWikiPageViewsForDays}days.json");
        }

        [Fact(Skip = "Tool to be used manually")]
        public async Task ToolMerge()
        {
            IFileSystem fs = new FileSystem();

            var cfg = new Configuration(fs).Read<WikitoolsCfg>();

            var stats1Path = cfg.StorageDirPath + "/wiki_stats_2021_01_19_30days.json";
            var stats1endDay = new DateDay(2021, 1, 19, DateTimeKind.Utc);
            var stats1startDay = stats1endDay.AddDays(-29);
            var stats2Path = cfg.StorageDirPath + "/wiki_stats_2021_02_06_30days.json";
            var stats2endDay = new DateDay(2021, 2, 6, DateTimeKind.Utc);
            var stats2startDay = stats1endDay.AddDays(-29);
            // var stats3Path = cfg.StorageDirPath + "/wiki_stats_2021_02_19_30days.json";
            // var stats4Path = cfg.StorageDirPath + "/wiki_stats_2021_02_27_30days.json";
            // var stats5Path = cfg.StorageDirPath + "/wiki_stats_2021_03_03_30days.json";
            // var stats6Path = cfg.StorageDirPath + "/wiki_stats_2021_03_17_30days.json";

            await Merge(
                fs,
                cfg,
                new[] { (stats1Path, stats1startDay, stats1endDay), (stats2Path, stats2startDay, stats2endDay) });
        }

        private static async Task Merge(IFileSystem fs, WikitoolsCfg cfg, (string, DateDay, DateDay)[] statsData)
        {
            var storage      = new MonthlyJsonFilesStorage(new Dir(fs, cfg.StorageDirPath));
            var januaryDate  = new DateMonth(2021, 1);
            var februaryDate = new DateMonth(2021, 2);
            var marchDate    = new DateMonth(2021, 3);

            var mergedStats = ValidWikiPagesStats.Merge(statsData.Select(s => DeserializeStats(fs, s)));

            await storage.Write(mergedStats, januaryDate, "merged_stats.json");

            await storage.Write(
                mergedStats.Trim(januaryDate),
                januaryDate,
                "date_2021_01_toolmerged.json");
            await storage.Write(
                mergedStats.Trim(februaryDate),
                februaryDate,
                "date_2021_02_toolmerged.json");
            await storage.Write(
                mergedStats.Trim(marchDate),
                marchDate,
                "date_2021_03_toolmerged.json");
        }

        private static ValidWikiPagesStats DeserializeStats(IFileSystem fs, (string stats, DateDay startDay, DateDay endDay) statsData) =>
            new(
                fs.ReadAllText(statsData.stats)
                    .FromJsonTo<WikiPageStats[]>(), statsData.startDay, statsData.endDay);
    }
}