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
            var os = new WindowsOS();

            var cfg = WikitoolsConfig.From("wikitools_config.json");

            var storage      = new MonthlyJsonFilesStorage(os.FileSystem, cfg.StorageDirPath);
            var januaryDate  = new DateTime(2021, 1, 1).ToUniversalTime();
            var backup1Path  = cfg.StorageDirPath + "/wiki_stats_2021_01_19_30days.json";
            var backup2Path  = cfg.StorageDirPath + "/wiki_stats_2021_02_06_30days.json";
            var backup1Stats = new ValidWikiPagesStats(
                JsonSerializer.Deserialize<WikiPageStats[]>(File.ReadAllText(backup1Path))!
                    .Select(WikiPageStats.FixNulls));
            var backup2Stats = new ValidWikiPagesStats(
                JsonSerializer.Deserialize<WikiPageStats[]>(File.ReadAllText(backup2Path))!
                    .Select(WikiPageStats.FixNulls));

            var mergedStats = backup1Stats.Merge(backup2Stats);
            var trimmedStats =
                mergedStats.Trim(januaryDate, januaryDate.AddMonths(1).AddDays(-1));

            await storage.Write(trimmedStats, januaryDate, "merged_stats_2021_01_19_2021_02_06.json");
        }
    }
}