using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record WikitoolsCfg(
        AzureDevOpsCfg AzureDevOpsCfg,
        // kj2 most of these config values should be pushed down, like StorageDirPath to ADO
        // and GitExecutablePath to lib (because GitLog is using it).
        string GitExecutablePath,
        string GitRepoClonePath,
        int GitLogDays,
        int AdoWikiPageViewsForDays,
        string[] ExcludedAuthors,
        string[] ExcludedPaths,
        DateDay MonthlyReportStartDay,
        DateDay MonthlyReportEndDay,
        int Top,
        string StorageDirPath) : IConfiguration
    {
        public Dir GitRepoCloneDir(IFileSystem fs) => new Dir(fs, GitRepoClonePath);
    }
}