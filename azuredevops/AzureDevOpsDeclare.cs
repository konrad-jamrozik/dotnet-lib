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
            DateTime utcNow,
            int? pageViewsForDaysMax = null)
        {
            var storageDir      = new Dir(fileSystem, storageDirPath);
            var storage         = AdoWikiPagesStatsStorage(storageDir, utcNow);
            var wikiWithStorage = AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysMax);
            return wikiWithStorage;
        }

        public AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            AdoWikiPagesStatsStorage storage,
            int? pageViewsForDaysMax = null) 
            => new AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysMax);

        public AdoWikiPagesStatsStorage AdoWikiPagesStatsStorage(Dir storageDir, DateTime utcNow)
            => new AdoWikiPagesStatsStorage(
                new MonthlyJsonFilesStorage(storageDir),
                utcNow);
    }
}