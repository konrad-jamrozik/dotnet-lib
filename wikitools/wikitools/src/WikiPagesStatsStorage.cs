using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public record WikiPagesStatsStorage(IOperatingSystem OS, string StorageDirPath)
    {
        public async Task<WikiPagesStatsStorage> Update(AdoWiki wiki, int pageViewsForDays)
        {
            var pageStats = await wiki.PagesStats(pageViewsForDays);
            (WikiPageStats[] lastMonth, WikiPageStats[] thisMonth) = SplitByMonth(pageStats);
            
            string lastMonthJson = GetPagesStatsJson(lastMonth);
            string thisMonthJson = GetPagesStatsJson(thisMonth);

            await Write(lastMonthJson, DateTime.UtcNow.AddMonths(-1));
            await Write(thisMonthJson, DateTime.UtcNow);

            return this;
        }

        // kja to implement, as follows, in pseudocode:
        //   var persistedStats = new PersistedWikiStats(storageDir);
        //   var lastMonthDays = ...; var thisMonthDays = ...
        //   return
        //     persistedStats.LastMonth.FilterTo(lastMonthDays)
        //     .Union
        //       persistedStats.ThisMonth.FilterTo(thisMonthDays)
        public Task<WikiPageStats[]> PagesStats(int pageViewsForDays)
        {
            var maxDate = FindMaxDate();
            var stats   = ReadStatsFromJson(maxDate);
            // kja to add: find second-last date (if necessary), read json, merge
            return Task.FromResult(stats);
        }

        private (WikiPageStats[] lastMonth, WikiPageStats[] thisMonth) SplitByMonth(WikiPageStats[] pageStats)
        {
            // kja NEXT
            return (new WikiPageStats[] { }, pageStats);
        }

        private async Task Write(string pageStatsJson, DateTime dateTime)
        {
            var storageDir = new Dir(OS.FileSystem, StorageDirPath);
            if (!storageDir.Exists())
                Directory.CreateDirectory(storageDir.Path);
            var filePath = Path.Join(StorageDirPath, $"date_{dateTime:yyy_MM}.json");
            await File.WriteAllTextAsync(filePath, pageStatsJson);
        }

        private static string GetPagesStatsJson(WikiPageStats[] pageStats)
        {
            return JsonSerializer.Serialize(pageStats,
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                });
        }

        private WikiPageStats[] ReadStatsFromJson(DateTime? maxDate)
        {
            var fileToReadName = $"date_{maxDate:yyy_MM}.json";
            var fileToReadPath = Path.Join(StorageDirPath, fileToReadName);
            var readJsonStr    = File.ReadAllText(fileToReadPath);
            return JsonSerializer.Deserialize<WikiPageStats[]>(readJsonStr)!;
        }

        private DateTime? FindMaxDate()
        {
            var       dir     = new Dir(OS.FileSystem, StorageDirPath);
            var       files   = Directory.EnumerateFiles(dir.Path);
            DateTime? maxDate = DateTime.MinValue;
            foreach (var file in files)
            {
                Console.Out.WriteLine("file: " + file);
                var dateMatch  = Regex.Match(file, "date_(.*)\\.json");
                var dateString = dateMatch.Groups[1].Value;
                var date       = DateTime.ParseExact(dateString, "yyyy_MM", CultureInfo.InvariantCulture);
                if (date > maxDate)
                {
                    maxDate = date;
                }
            }

            return maxDate;
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
// Later: think about decoupling the logic from FileSystem; maybe arbitrary storage via streams/writers
// would make more sense. At least the merging and splitting algorithm should be decoupled from file system.