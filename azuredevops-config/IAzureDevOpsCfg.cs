using Wikitools.Lib.Json;

namespace Wikitools.AzureDevOps.Config;

public interface IAzureDevOpsCfg : IConfiguration
{
    public string AdoWikiUri();

    // Assumed to contain a PAT token of a user that has access to the wiki with url AdoWikiUri
    public string AdoPatEnvVar();
} 