﻿using System;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Storage;

namespace Wikitools
{
    public static class Declare
    {
        public static GitLog GitLog(IOperatingSystem os, string gitRepoDirPath, string gitExecutablePath)
        {
            var repo = new GitRepository(
                new GitBashShell(os, gitExecutablePath),
                gitRepoDirPath
            );
            var gitLog = new GitLog(repo);
            return gitLog;
        }

        public static IAdoWiki Wiki(IAdoApi adoApi, string wikiUri, string patEnvVar) 
            => new AdoWiki(adoApi, new AdoWikiUri(wikiUri), patEnvVar);

        public static IAdoWiki WikiWithStorage(
            IAdoApi adoApi,
            string wikiUri,
            string patEnvVar,
            IFileSystem filesystem,
            string storageDirPath,
            DateTime now)
        {
            var storage = new WikiPagesStatsStorage(new MonthlyJsonFilesStorage(filesystem, storageDirPath), now);
            var wikiWithStorage = new AdoWikiWithStorage(Wiki(adoApi, wikiUri, patEnvVar), storage);
            return wikiWithStorage;
        }
    }
}