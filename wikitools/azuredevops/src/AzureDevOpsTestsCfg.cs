using System;
using Wikitools.Lib.Configuration;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.AzureDevOps
{
    // kja dedup with WikitoolsConfig
    public record AzureDevOpsTestsCfg(
        string AdoWikiUri,
        // Assumed to contain a PAT token of a user that has access to the wiki with url AdoWikiUri
        string AdoPatEnvVar,
        // kja move these test items to a test config class.
        string TestStorageDirPath,
        // Assumed to point to valid page in the ADO wiki with url AdoWikiUrl
        int TestAdoWikiPageId) : IConfiguration
    {
        public static AzureDevOpsTestsCfg From(IFileSystem fs, string cfgFileName = "config.json")
        {
            var cfgFilePath = FindConfigFilePath(fs, cfgFileName);
            return cfgFilePath != null && fs.FileExists(cfgFilePath)
                ? fs.ReadAllBytes(cfgFilePath).FromJsonTo<AzureDevOpsTestsCfg>()
                : throw new Exception($"Failed to find {cfgFileName}.");
        }

        private static string? FindConfigFilePath(IFileSystem fs, string cfgFileName)
        {
            var dir = fs.CurrentDir;

            var cfgFilePath = dir.JoinPath(cfgFileName);

            while (!fs.FileExists(cfgFilePath) && dir.Parent != null)
            {
                dir = dir.Parent;
                cfgFilePath = dir.JoinPath(cfgFileName);
            }

            return dir.Parent != null ? cfgFilePath : null;
        }
    }
}