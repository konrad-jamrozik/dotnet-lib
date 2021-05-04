using Wikitools.Lib.Configuration;

namespace Wikitools.AzureDevOps
{
    // kja 5.2 dedup with WikitoolsConfig
    public record AzureDevOpsTestsCfg(
        string AdoWikiUri,
        // Assumed to contain a PAT token of a user that has access to the wiki with url AdoWikiUri
        string AdoPatEnvVar,
        // kja 5.1 move these test items to a test config class.
        string TestStorageDirPath,
        // Assumed to point to valid page in the ADO wiki with url AdoWikiUrl
        int TestAdoWikiPageId) : IConfiguration
    {
    }
}