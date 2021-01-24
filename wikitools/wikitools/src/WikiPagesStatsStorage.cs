﻿using System;
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
        public async Task<WikiPagesStatsStorage> Update(AdoWiki wiki)
        {
            // kja dehardcode constant
            var pageStats = await wiki.PagesStats(5);

            // kja deduplicate serialization logic with JsonDiff
            string pageStatsJson = GetPageStatsJson(pageStats);

            await Write(pageStatsJson);

            return this;

            // kja NEXT curr work.
            // Proven to be able to write to file system. 
            // Next tasks:
            // - replace File.WriteAllTextAsync. Introduce File abstraction or similar,
            // that depends on OS.FileSystem. Make it create Dirs as needed when writing out.
            // possibly pass it as Options param, like CreateMissingDirs.
            // - implement logic as follows, in pseudocode:
            //
            //   var persistedStats = new PersistedWikiStats(storageDir);
            //   var lastDate = await persistedStats.LastPersistedDate; // need to await sa it reads from FileSystem
            //   var pageStats = await wiki.PagesStats(DateTime.now - lastDate)
            //   var (lastMonthStats, thisMonthStats) = pageStats.SplitByMonth()
            //   await new MergedStats(lastMonthStats, persistedStats.LastMonth).Persist();
            //   await new MergedStats(thisMonthStats, persistedStats.ThisMonth).Persist();
            //   // internally, the two above will deserialize the persisted JSON into WikiPageStats,
            //   // then merge with ___monthStats WikiPageStats, and then serialize back to the file system.
            //
            // Later: think about decoupling the logic from FileSystem; maybe arbitrary storage via streams/writers
            // would make more sense. At least the merging and splitting algorithm should be decoupled from file system.
        }

        private async Task Write(string pageStatsJson)
        {
            var storageDir = new Dir(OS.FileSystem, StorageDirPath);
            if (!storageDir.Exists())
                Directory.CreateDirectory(storageDir.Path);
            var filePath = Path.Join(StorageDirPath, $"date_{DateTime.UtcNow:yyy_MM_dd}.json");
            await File.WriteAllTextAsync(filePath, pageStatsJson);
        }

        private static string GetPageStatsJson(WikiPageStats[] pageStats)
        {
            return JsonSerializer.Serialize(pageStats,
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                });
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
            return Task.FromResult(stats);
        }

        private WikiPageStats[] ReadStatsFromJson(DateTime? maxDate)
        {
            var fileToReadName = $"date_{maxDate:yyy_MM_dd}.json";
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
                var date       = DateTime.ParseExact(dateString, "yyyy_MM_dd", CultureInfo.InvariantCulture);
                if (date > maxDate)
                {
                    maxDate = date;
                }
            }

            return maxDate;
        }
    }
}