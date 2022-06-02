using System.Threading.Tasks;
using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps.Tests;

public class AdoWikiPagesStatsStorageDeclare
{
    public static AdoWikiPagesStatsStorage New(
        IAzureDevOpsTestsCfg adoTestsCfg,
        DateDay currentDay)
    {
        var storageDecl = new AzureDevOps.AdoWikiPagesStatsStorageDeclare();
        var storage = storageDecl.AdoWikiPagesStatsStorage(
            adoTestsCfg.TestStorageDir(),
            currentDay);
        return storage;
    }

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
        var decl       = new AzureDevOps.AdoWikiPagesStatsStorageDeclare();
        var storage    = decl.AdoWikiPagesStatsStorage(storageDir, utcNow);
        if (storedStats != null)
            storage = await storage.ReplaceWith(storedStats);
        return storage;
    }
}