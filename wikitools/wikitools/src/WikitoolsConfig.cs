using System;
using System.IO;
using System.Text.Json;

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
        public static WikitoolsConfig From(string cfgFileName)
        {
            // kja use FileSystem
            var cfgFilePath = FindConfigFilePath(cfgFileName);
            WikitoolsConfig? cfg = null;

            if (cfgFilePath != null && File.Exists(cfgFilePath))
                // kja intro method for Deserialize + ReadAllBytes
                cfg = JsonSerializer.Deserialize<WikitoolsConfig>(File.ReadAllBytes(cfgFilePath),
                    new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });
            
            if (cfg is null)
                throw new Exception($"Failed to find {cfgFileName}.");

            return cfg;
        }

        private static string? FindConfigFilePath(string cfgFileName)
        {
            // kja use FileSystem
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var cfgFilePath = Path.Combine(dir.FullName, cfgFileName);

            while (!File.Exists(cfgFilePath) && dir.Parent != null)
            {
                dir = dir.Parent;
                cfgFilePath = Path.Combine(dir.FullName, cfgFileName);
            }

            return dir.Parent != null 
                ? cfgFilePath 
                : null;
        }
    }
}