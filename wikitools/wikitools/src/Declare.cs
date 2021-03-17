using System;
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

        public static IAdoWiki Wiki(IAdoWikiApi adoWikiApi, string patEnvVar) 
            => new AdoWiki(adoWikiApi, patEnvVar);

        public static IAdoWiki WikiWithStorage(
            IAdoWikiApi adoWikiApi,
            string patEnvVar,
            IFileSystem filesystem,
            string storageDirPath,
            DateTime now)
        {
            var storage = new WikiPagesStatsStorage(new MonthlyJsonFilesStorage(filesystem, storageDirPath), now);
            var wikiWithStorage = new AdoWikiWithStorage(Wiki(adoWikiApi, patEnvVar), storage);
            return wikiWithStorage;
        }
    }
}