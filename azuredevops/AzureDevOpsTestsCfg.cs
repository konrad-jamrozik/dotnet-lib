using Wikitools.Lib.Json;

namespace Wikitools.AzureDevOps;

public record AzureDevOpsTestsCfg(
    string TestStorageDirPath,
    // Assumed to point to valid page in the ADO wiki with url AzureDevOpsCfg.AdoWikiUrl
    int TestAdoWikiPageId) : IConfiguration;