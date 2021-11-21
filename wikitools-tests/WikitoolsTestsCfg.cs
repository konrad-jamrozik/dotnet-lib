using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;

namespace Wikitools.Tests;

public record WikitoolsTestsCfg(
    AzureDevOpsCfg AzureDevOpsCfg,
    string TestGitRepoClonePath,
    string TestStorageDirPath,
    int TestGitRepoExpectedPathsMinPageCount,
    int TestGitRepoExpectedPathsMinPageViewsCount) : IConfiguration;