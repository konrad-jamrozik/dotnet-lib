using Wikitools.Lib.Json;

namespace Wikitools.AzureDevOps
{
    public record AzureDevOpsCfg(
        string AdoWikiUri,
        // Assumed to contain a PAT token of a user that has access to the wiki with url AdoWikiUri
        string AdoPatEnvVar) : IConfiguration
    {
    }
}