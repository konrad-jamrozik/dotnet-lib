using Wikitools.Lib.Json;

namespace Wikitools.AzureDevOps.Config;

public interface IAzureDevOpsTestsCfg : IConfiguration
{
    public IAzureDevOpsCfg AzureDevOpsCfg();

    public string TestStorageDirPath();

    public int TestAdoWikiPageId();
}