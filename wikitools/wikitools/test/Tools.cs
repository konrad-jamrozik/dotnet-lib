using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
using Xunit;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.Tests
{
    public class Tools
    {
        [Fact(Skip = "For one-off experiments")]
        public void Scratchpad()
        {
        }

        [Fact(Skip = "Tool to be used manually")]
        public async Task ToolGetWikiStats()
        {
            ITimeline       timeline = new Timeline();
            IFileSystem     fs       = new FileSystem();
            IEnvironment    env      = new Environment();
            WikitoolsConfig cfg      = WikitoolsConfig.From(fs);
            IAdoWiki        adoWiki  = new AdoWiki(cfg.AdoConfig.AdoWikiUri, cfg.AdoConfig.AdoPatEnvVar, env);

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

            var cfg = WikitoolsConfig.From(fs);

            var stats1Path = cfg.StorageDirPath + "/wiki_stats_2021_01_19_30days.json";
            var stats2Path = cfg.StorageDirPath + "/wiki_stats_2021_02_06_30days.json";
            var stats3Path = cfg.StorageDirPath + "/wiki_stats_2021_02_19_30days.json";
            var stats4Path = cfg.StorageDirPath + "/wiki_stats_2021_02_27_30days.json";
            var stats5Path = cfg.StorageDirPath + "/wiki_stats_2021_03_03_30days.json";
            var stats6Path = cfg.StorageDirPath + "/wiki_stats_2021_03_17_30days.json";

            await Merge(
                fs,
                cfg,
                new[] { stats1Path, stats2Path, stats3Path, stats4Path, stats5Path, stats6Path });
        }

        private static async Task Merge(IFileSystem fs, WikitoolsConfig cfg, string[] statsPaths)
        {
            var storage      = new MonthlyJsonFilesStorage(new Dir(fs, cfg.StorageDirPath));
            var januaryDate  = new DateMonth(2021, 1);
            var februaryDate = new DateMonth(2021, 2);
            var marchDate    = new DateMonth(2021, 3);

            var mergedStats = statsPaths.Select(s => DeserializeStats(fs, s))
                // This is O(n^2) while it could be O(n), but for now this is good enough.
                .Aggregate((prevStats, currentStats) => prevStats.Merge(currentStats));

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

        private static ValidWikiPagesStats DeserializeStats(IFileSystem fs, string stats) =>
            new(
                fs.ReadAllText(stats)
                    .FromJsonTo<WikiPageStats[]>());
    }
}