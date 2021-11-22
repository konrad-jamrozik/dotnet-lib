using System;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public record WikitoolsCfg(
        AzureDevOpsCfg AzureDevOpsCfg,
        string GitExecutablePath,
        string GitRepoClonePath,
        int GitLogDays,
        int AdoWikiPageViewsForDays,
        string[] ExcludedAuthors,
        string[] ExcludedPaths,
        DateTime MonthlyReportStartDate,
        DateTime MonthlyReportEndDate,
        int Top,
        string StorageDirPath) : IConfiguration
    {
        public Dir GitRepoCloneDir(IFileSystem fs) => new Dir(fs, GitRepoClonePath);
    }
}