using System.Threading.Tasks;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps.Tests
{
    public record AzureDevOpsTestsDeclare(Declare Decl)
    {
        public async Task<AdoWikiPagesStatsStorage> AdoWikiPagesStatsStorage(
            DateDay utcNow,
            ValidWikiPagesStatsForMonth? storedStats = null)
        {
            var fs         = new SimulatedFileSystem();
            var storageDir = fs.NextSimulatedDir();
            var storage    = Decl.AdoWikiPagesStatsStorage(utcNow, storageDir);
            if (storedStats != null)
                storage = await storage.OverwriteWith(storedStats);
            return storage;
        }
    }
}