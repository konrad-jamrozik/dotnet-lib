using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.AzureDevOps.Config;

public interface IAzureDevOpsTestsCfg : IConfiguration
{
    public IAzureDevOpsCfg AzureDevOpsCfg();
    public string TestStorageDirPath();
    public int TestAdoWikiPageId();
    public Dir TestStorageDir(IFileSystem fs) => new Dir(fs, TestStorageDirPath());
}