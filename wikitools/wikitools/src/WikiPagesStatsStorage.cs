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

            var path       = Path.Join(StorageDirPath, $"date_{DateTime.UtcNow:yyy_MM_dd}.json");
            var storageDir = new Dir(OS.FileSystem, path);
            await File.WriteAllTextAsync(storageDir.Path, serialized);

            return this;
        }

        public Task<WikiPageStats[]> PagesStats(int pageViewsForDays) 
            => Wiki.PagesStats(pageViewsForDays);
    }
}