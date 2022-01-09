﻿using Wikitools.AzureDevOps.Config;
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

    // kja make it use AzureDevOpsTestsCfg.TestStorageDir.
    // This will require for this class to use AzureDevOpsTestsCfg instead of AzureDevOpsCfg
    public Dir TestStorageDir(IFileSystem fs) => new Dir(fs, TestStorageDirPath());
}