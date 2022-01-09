using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Config;

public interface IWikitoolsTestsCfg : IConfiguration
{
    public IWikitoolsCfg WikitoolsCfg();
    public IAzureDevOpsTestsCfg AzureDevOpsTestsCfg();
    public int TestGitRepoExpectedPathsMinPageCount();
    public int TestGitRepoExpectedPathsMinPageViewsCount();

    public IAzureDevOpsCfg AzureDevOpsCfg => AzureDevOpsTestsCfg().AzureDevOpsCfg();

    public Dir TestStorageDir(IFileSystem fs)
        => new Dir(fs, AzureDevOpsTestsCfg().TestStorageDirPath());
}