using System;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // - Reason: hydrated by a call to FromJsonTo<WikitoolsConfig>() in Wikitools.WikitoolsConfig.From
    public record WikitoolsConfig(
        string GitExecutablePath,
        string GitRepoClonePath,
        int GitLogDays,
        string AdoWikiUri,
        string AdoPatEnvVar,
        int AdoWikiPageViewsForDays,
        string[] ExcludedAuthors,
        string[] ExcludedPaths,
        DateTime MonthlyReportStartDate,
        DateTime MonthlyReportEndDate,
        int Top,
        string StorageDirPath,
        string TestStorageDirPath)
    {
        public static WikitoolsConfig From(IFileSystem fs, string cfgFileName = "wikitools_config.json")
        {
            var cfgFilePath = FindConfigFilePath(fs, cfgFileName);
            return cfgFilePath != null && fs.FileExists(cfgFilePath)
                ? fs.ReadAllBytes(cfgFilePath).FromJsonTo<WikitoolsConfig>()
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