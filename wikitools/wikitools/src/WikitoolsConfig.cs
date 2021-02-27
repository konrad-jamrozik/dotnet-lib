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
        string StorageDirPath)
    {
        public static WikitoolsConfig From(IFileSystem fs, string cfgFileName)
        {
            var cfgFilePath = FindConfigFilePath(fs, cfgFileName);
            return cfgFilePath != null && fs.FileExists(cfgFilePath)
                ? fs.ReadAllBytes(cfgFilePath).FromJsonTo<WikitoolsConfig>()
                : throw new Exception($"Failed to find {cfgFileName}.");
        }

        private static string? FindConfigFilePath(IFileSystem fs, string cfgFileName)
        {
            var dir = fs.CurrentDirectoryInfo;

            var cfgFilePath = fs.CombinePath(dir.FullName, cfgFileName);

            while (!fs.FileExists(cfgFilePath) && dir.Parent != null)
            {
                dir = dir.Parent;
                cfgFilePath = fs.CombinePath(dir.FullName, cfgFileName);
            }

            return dir.Parent != null ? cfgFilePath : null;
        }
    }
}