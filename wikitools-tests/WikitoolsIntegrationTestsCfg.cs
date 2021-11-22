using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Tests;

public record WikitoolsIntegrationTestsCfg(
    WikitoolsCfg WikitoolsCfg,
    AzureDevOpsCfg AzureDevOpsCfg,
    string TestStorageDirPath,
    int TestGitRepoExpectedPathsMinPageCount,
    int TestGitRepoExpectedPathsMinPageViewsCount) : IConfiguration
{
    public Dir TestStorageDir(IFileSystem fs) => new Dir(fs, TestStorageDirPath);
}