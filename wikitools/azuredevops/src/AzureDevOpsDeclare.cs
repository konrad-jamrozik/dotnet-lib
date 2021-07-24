using System;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps
{
    public class AzureDevOpsDeclare
    {
        public GitLog GitLog(IOperatingSystem os, Dir gitRepoDir, string gitExecutablePath)
        {
            var repo = new GitRepository(
                new GitBashShell(os, gitExecutablePath),
                gitRepoDir
            );
            var gitLog = new GitLog(repo);
            return gitLog;
        }

        public AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            IFileSystem fileSystem,
            string storageDirPath,
            DateTime now,
            int? pageViewsForDaysWikiLimit = null)
        {
            var storageDir      = new Dir(fileSystem, storageDirPath);
            var storage         = AdoWikiPagesStatsStorage(storageDir, now);
            var wikiWithStorage = AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysWikiLimit);
            return wikiWithStorage;
        }

        public AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            AdoWikiPagesStatsStorage storage,
            int? pageViewsForDaysWikiLimit = null) 
            => new AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysWikiLimit);

        public AdoWikiPagesStatsStorage AdoWikiPagesStatsStorage(Dir storageDir, DateTime now) =>
            new(
                new MonthlyJsonFilesStorage(storageDir),
                now);
    }
}