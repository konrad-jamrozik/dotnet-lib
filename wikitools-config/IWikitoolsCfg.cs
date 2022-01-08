using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Config;

public interface IWikitoolsCfg : IConfiguration
{
    public IAzureDevOpsCfg AzureDevOpsCfg();

    // kj2 most of these config values should be pushed down, like StorageDirPath to ADO
    // and GitExecutablePath to lib (because GitLog is using it).
    public string GitExecutablePath();
    public string GitRepoClonePath();
    public int GitLogDays();
    public int AdoWikiPageViewsForDays();
    public string[] ExcludedAuthors();
    public string[] ExcludedPaths();
    public DateDay MonthlyReportStartDay();
    public DateDay MonthlyReportEndDay();
    public int Top();
    public string StorageDirPath();
    public Dir GitRepoCloneDir(IFileSystem fs) => new Dir(fs, GitRepoClonePath());
}