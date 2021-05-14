using Wikitools.Lib.Json;

namespace Wikitools.AzureDevOps
{
    // kja 5.1 split off AzureDevOpsTestsCfg, fix naming, and dedup with WikitoolsConfig
    public record AzureDevOpsTestsCfg(
        string AdoWikiUri,
        // Assumed to contain a PAT token of a user that has access to the wiki with url AdoWikiUri
        string AdoPatEnvVar,
        
        string TestStorageDirPath,
        // Assumed to point to valid page in the ADO wiki with url AdoWikiUrl
        int TestAdoWikiPageId) : IConfiguration
    {
    }
}