using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Config;

public interface IWikitoolsCfg : IConfiguration
{
    public IAzureDevOpsCfg AzureDevOpsCfg();
    public string GitExecutablePath();
    public string GitRepoClonePath();
    public int GitLogDays();
    public int AdoWikiPageViewsForDays();
    public string[] ExcludedAuthors();
    public string[] ExcludedPaths();
    public DaySpan MonthlyReportDaySpan();
    public int Top();
    public string StorageDirPath();
    public Dir GitRepoCloneDir(IFileSystem fs) => new Dir(fs, GitRepoClonePath());
}