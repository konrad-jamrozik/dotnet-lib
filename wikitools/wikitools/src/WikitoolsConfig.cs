﻿using System;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools
{
    // kj3 idea for config:
    // - Have the records by themselves, with no From or file discovery logic
    // - Have a composite record, nesting all the sub-records
    // - Have a Configuration class, that uses reflection to crawl the composite record
    // and hydrate it from .json files. Probably something like:
    // WikitoolsConfig cfg = new Configuration(rootDir).ReadFromJsonFiles<WikitoolsConfig>();
    //
    // alternatively: make WikitoolsConfig implement IConfiguration and provide an extension method that hydrates it:
    // cfg.From(dir)
    // - This will likely not work as such extension method would have to be over generic type IConfiguration,
    //   which will not know how to exactly hydrate.
    //
    // Note that here the file names are implicitly deduced from the type names.
    // See also:
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0

    // ReSharper disable once ClassNeverInstantiated.Global
    // - Reason: hydrated by a call to FromJsonTo<WikitoolsConfig>() in Wikitools.WikitoolsConfig.From
    public record WikitoolsConfig(
        AdoConfig AdoConfig,
        string GitExecutablePath,
        string GitRepoClonePath,
        int GitLogDays,
        int AdoWikiPageViewsForDays,
        string[] ExcludedAuthors,
        string[] ExcludedPaths,
        DateTime MonthlyReportStartDate,
        DateTime MonthlyReportEndDate,
        int Top,
        string StorageDirPath)
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