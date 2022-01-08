using Wikitools.Lib.Json;

namespace Wikitools.AzureDevOps.Config;

public interface IAzureDevOpsTestsCfg : IConfiguration
{
    public string TestStorageDirPath();

    public int TestAdoWikiPageId();
}