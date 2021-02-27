using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Xunit;
using static Wikitools.Declare;

namespace Wikitools.Tests
{
    public class Tools
    {
        [Fact(Skip = "Tool to be used manually")]
        public async Task ToolGetWikiStats()
        {
            var timeline = new Timeline();
            var os       = new WindowsOS();
            var adoApi   = new AdoApi(os.Environment);
            var cfg      = WikitoolsConfig.From("wikitools_config.json");

            var wiki            = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar);
            var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            var storage = new MonthlyJsonFilesStorage(os.FileSystem, cfg.StorageDirPath);

            await storage.Write(await pagesViewsStats,
                timeline.UtcNow,
                $"wiki_stats_{timeline.UtcNow:yyy_MM_dd}_{cfg.AdoWikiPageViewsForDays}days.json");
        }

        // kja do not make calls to File.ReadAllText directly. Instead, obtain them from os.
        [Fact(Skip = "Tool to be used manually")]
        public async Task ToolMerge()
        {
            var cfg        = WikitoolsConfig.From("wikitools_config.json");
            var stats1Path = cfg.StorageDirPath + "/wiki_stats_2021_01_19_30days.json";
            var stats2Path = cfg.StorageDirPath + "/wiki_stats_2021_02_06_30days.json";
            var stats3Path = cfg.StorageDirPath + "/wiki_stats_2021_02_19_30days.json";
            var stats4Path = cfg.StorageDirPath + "/wiki_stats_2021_02_27_30days.json";

            await Merge(cfg, new[] { stats1Path, stats2Path, stats3Path, stats4Path});
        }

        private static async Task Merge(WikitoolsConfig cfg, string[] statsPaths)
        {
            var os = new WindowsOS();

            var storage     = new MonthlyJsonFilesStorage(os.FileSystem, cfg.StorageDirPath);
            var januaryDate = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var mergedStats = statsPaths.Select(DeserializeStats)
                .Aggregate((prevStats, currentStats) => prevStats.Merge(currentStats));

            await storage.Write(mergedStats, januaryDate, "merged_stats.json");

            await storage.Write(
                mergedStats.Trim(januaryDate, januaryDate.AddMonths(1).AddDays(-1)),
                januaryDate,
                "date_2021_01_toolmerged.json");
        }

        private static ValidWikiPagesStats DeserializeStats(string prevStatsPath) =>
            new(
                JsonSerializer.Deserialize<WikiPageStats[]>(File.ReadAllText(prevStatsPath))!
                    .Select(WikiPageStats.FixNulls));
    }
}