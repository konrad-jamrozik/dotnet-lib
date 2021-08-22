using System.Threading.Tasks;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps.Tests
{
    public record AzureDevOpsTestsDeclare(AzureDevOpsDeclare Decl)
    {
        public Task<AdoWikiPagesStatsStorage> AdoWikiPagesStatsStorage(
            DateDay utcNow,
            ValidWikiPagesStatsForMonth? storedStats = null)
            => AdoWikiPagesStatsStorage(utcNow, (ValidWikiPagesStats?) storedStats);

        public async Task<AdoWikiPagesStatsStorage> AdoWikiPagesStatsStorage(
            DateDay utcNow,
            ValidWikiPagesStats? storedStats = null)
        {
            var fs         = new SimulatedFileSystem();
            var storageDir = fs.NextSimulatedDir();
            var storage    = Decl.AdoWikiPagesStatsStorage(storageDir, utcNow);
            if (storedStats != null)
                storage = await storage.ReplaceWith(storedStats);
            return storage;
        }
    }
}