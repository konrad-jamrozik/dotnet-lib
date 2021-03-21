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

        public static IAdoWiki WikiWithStorage(
            IAdoWiki adoWiki,
            IFileSystem fileSystem,
            string storageDirPath,
            DateTime now)
        {
            var storage = new WikiPagesStatsStorage(new MonthlyJsonFilesStorage(new Dir(fileSystem, storageDirPath)), now);

            var wikiWithStorage = new AdoWikiWithStorage(adoWiki, storage);
            return wikiWithStorage;
        }
    }
}