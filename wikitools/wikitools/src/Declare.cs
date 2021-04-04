using System;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Storage;

namespace Wikitools
{
    public static class Declare
    {
        public static GitLog GitLog(IOperatingSystem os, Dir gitRepoDir, string gitExecutablePath)
        {
            var repo = new GitRepository(
                new GitBashShell(os, gitExecutablePath),
                gitRepoDir
            );
            var gitLog = new GitLog(repo);
            return gitLog;
        }

        public static AdoWikiWithStorage WikiWithStorage(
            IAdoWiki adoWiki,
            IFileSystem fileSystem,
            string storageDirPath,
            DateTime now,
            int? pageViewsForDaysWikiLimit = null)
        {
            var storageDir      = new Dir(fileSystem, storageDirPath);
            var storage         = WikiPagesStatsStorage(now, storageDir);
            var wikiWithStorage = AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysWikiLimit);
            return wikiWithStorage;
        }

        public static AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            WikiPagesStatsStorage storage,
            int? pageViewsForDaysWikiLimit = null) =>
            new(adoWiki, storage, pageViewsForDaysWikiLimit);

        public static WikiPagesStatsStorage WikiPagesStatsStorage(DateTime now, Dir storageDir) =>
            new(
                new MonthlyJsonFilesStorage(storageDir),
                now);
    }
}