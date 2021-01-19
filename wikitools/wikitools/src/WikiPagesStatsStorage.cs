using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public record WikiPagesStatsStorage(IOperatingSystem OS, string StorageDirPath, AdoWiki Wiki)
    {
        public async Task<WikiPagesStatsStorage> Update(AdoWiki wiki)
        {
            // kja dehardcode constant
            var pageStats = await wiki.PagesStats(5);

            // kja deduplicate serialization logic with JsonDiff
            string serialized = JsonSerializer.Serialize(pageStats,
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                });

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
            // would make more sense. At least the merging and spliting algorithm should be decoupled from file system.
            var storageDir = new Dir(OS.FileSystem, StorageDirPath);
            if (!storageDir.Exists())
                Directory.CreateDirectory(storageDir.Path);
            var filePath = Path.Join(StorageDirPath, $"date_{DateTime.UtcNow:yyy_MM_dd}.json");
            await File.WriteAllTextAsync(filePath, serialized);

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
            => Wiki.PagesStats(pageViewsForDays);
    }
}