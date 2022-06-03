using System.Threading.Tasks;
using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.OS;

namespace Wikitools.AzureDevOps.Tests;

public class AdoWikiPagesStatsStorageDeclare
{
    public static AdoWikiPagesStatsStorage New(IAzureDevOpsTestsCfg adoTestsCfg)
    {
        var storageDecl = new AzureDevOps.AdoWikiPagesStatsStorageDeclare();
        var storage = storageDecl.New(
            adoTestsCfg.TestStorageDir());
        return storage;
    }

    public async Task<AdoWikiPagesStatsStorage> New(ValidWikiPagesStats? storedStats = null)
    {
        var fs         = new SimulatedFileSystem();
        var storageDir = fs.NextSimulatedDir();
        var decl       = new AzureDevOps.AdoWikiPagesStatsStorageDeclare();
        var storage    = decl.New(storageDir);
        if (storedStats != null)
            storage = await storage.ReplaceWith(storedStats);
        return storage;
    }
}