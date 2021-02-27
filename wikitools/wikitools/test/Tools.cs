using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
using Xunit;
using static Wikitools.Declare;

namespace Wikitools.Tests
{
    public class Tools
    {
        [Fact(Skip = "Tool to be used manually")]
        public async Task ToolGetWikiStats()
        {
            ITimeline        timeline = new Timeline();
            IOperatingSystem os       = new WindowsOS();
            IAdoApi          adoApi   = new AdoApi(os.Environment);

            var cfg = WikitoolsConfig.From(os.FileSystem, "wikitools_config.json");

            var wiki            = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar);
            var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            var storage = new MonthlyJsonFilesStorage(os.FileSystem, cfg.StorageDirPath);

            await storage.Write(await pagesViewsStats,
                timeline.UtcNow,
                $"wiki_stats_{timeline.UtcNow:yyy_MM_dd}_{cfg.AdoWikiPageViewsForDays}days.json");
        }

        [Fact(Skip = "Tool to be used manually")]
        public async Task ToolMerge()
        {
            IOperatingSystem os = new WindowsOS();

            var cfg = WikitoolsConfig.From(os.FileSystem, "wikitools_config.json");

            var stats1Path = cfg.StorageDirPath + "/wiki_stats_2021_01_19_30days.json";
            var stats2Path = cfg.StorageDirPath + "/wiki_stats_2021_02_06_30days.json";
            var stats3Path = cfg.StorageDirPath + "/wiki_stats_2021_02_19_30days.json";
            var stats4Path = cfg.StorageDirPath + "/wiki_stats_2021_02_27_30days.json";

            await Merge(os.FileSystem, cfg, new[] { stats1Path, stats2Path, stats3Path, stats4Path});
        }

        private static async Task Merge(IFileSystem fs, WikitoolsConfig cfg, string[] statsPaths)
        {
            var storage     = new MonthlyJsonFilesStorage(fs, cfg.StorageDirPath);
            var januaryDate = new DateTime(2021, 1, 1).Utc();

            var mergedStats = statsPaths.Select(s => DeserializeStats(fs, s))
                .Aggregate((prevStats, currentStats) => prevStats.Merge(currentStats));

            await storage.Write(mergedStats, januaryDate, "merged_stats.json");

            await storage.Write(
                mergedStats.Trim(januaryDate),
                januaryDate,
                "date_2021_01_toolmerged.json");
        }

        private static ValidWikiPagesStats DeserializeStats(IFileSystem fs, string prevStatsPath) =>
            new(
                // kja intro method for Desrialize + ReadAllText
                JsonSerializer.Deserialize<WikiPageStats[]>(fs.ReadAllText(prevStatsPath))!
                    .Select(WikiPageStats.FixNulls));
    }
}