using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Config;

public interface IWikitoolsIntegrationTestsCfg : IConfiguration
{
    public IWikitoolsCfg WikitoolsCfg();
    public IAzureDevOpsCfg AzureDevOpsCfg();
    public string TestStorageDirPath();
    public int TestGitRepoExpectedPathsMinPageCount();
    public int TestGitRepoExpectedPathsMinPageViewsCount();

    public Dir TestStorageDir(IFileSystem fs) => new Dir(fs, TestStorageDirPath());
}