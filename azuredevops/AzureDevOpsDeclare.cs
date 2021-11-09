using System;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps
{
    // kj2 cohesion of this class is low. Possibly split into
    // GitLogDecl and AdoWikiWithStorageDecl
    // Other possible suffixed: Ctor, Def, Scaffolding, Blueprint, Chart (a'la Helm Chart), Factory, DI (for Dep. Inj).
    // With that, for each class Foo, we will have:
    // Foo, FooChart, FooTests, FooIntegrationTests, FooFixture, FooTestData, FooTestDataFixture
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