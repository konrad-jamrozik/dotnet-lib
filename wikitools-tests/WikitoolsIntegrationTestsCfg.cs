using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;

namespace Wikitools.Tests;

public record WikitoolsIntegrationTestsCfg(
    WikitoolsCfg WikitoolsCfg,
    AzureDevOpsCfg AzureDevOpsCfg,
    string TestStorageDirPath,
    int TestGitRepoExpectedPathsMinPageCount,
    int TestGitRepoExpectedPathsMinPageViewsCount) : IConfiguration;