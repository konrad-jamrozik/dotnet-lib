using System;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;

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
        string StorageDirPath) : IConfiguration;
}