using System;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // - Reason: hydrated by a call to JsonSerializer.Deserialize in Wikitools.WikitoolsConfig.From
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
            // kja use FileSystem
            var cfgFilePath = FindConfigFilePath(fs, cfgFileName);
            WikitoolsConfig? cfg = null;

            if (cfgFilePath != null && fs.FileExists(cfgFilePath))
                cfg = fs.ReadAllBytes(cfgFilePath).FromJsonTo<WikitoolsConfig>();
            
            if (cfg is null)
                throw new Exception($"Failed to find {cfgFileName}.");

            return cfg;
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