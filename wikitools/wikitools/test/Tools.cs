using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
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
            var adoApi   = new AdoApi();
            var os       = new WindowsOS();
            var cfg      = WikitoolsConfig.From("wikitools_config.json");

            var wiki            = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar);
            var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            var storage = new MonthlyJsonFilesStorage(os, cfg.StorageDirPath);

            await storage.Write(await pagesViewsStats, timeline.UtcNow);
        }

        [Fact(Skip = "Tool to be used manually")]
        public async Task ToolMerge()
        {
            var cfg = WikitoolsConfig.From("wikitools_config.json");

            var storage           = new MonthlyJsonFilesStorage(new WindowsOS(), cfg.StorageDirPath);
            var janDate           = new DateTime(2021, 1, 1);
            var januaryStats      = storage.Read<WikiPageStats[]>(janDate);
            var backedUpStatsPath = cfg.StorageDirPath + "/wiki_stats_2021_01_19_30days.json";
            var backedUpStats     = JsonSerializer.Deserialize<WikiPageStats[]>(File.ReadAllText(backedUpStatsPath))!;

            var mergedStats       = WikiPagesStatsStorage.Merge(januaryStats, backedUpStats);

            await storage.Write(mergedStats, janDate);
        }
    }
}