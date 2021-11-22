using System;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps
{
    // kj2 cohesion of this class is low. Possibly split into
    // GitLogDecl and AdoWikiWithStorageDecl
    // Other possible suffixes: Ctor, Def, Scaffolding, Blueprint, Chart (a'la Helm Chart), Factory, DI (for Dep. Inj).
    // With that, for each class Foo, we will have:
    // Foo, FooChart, FooTests, FooIntegrationTests, FooFixture, FooTestData, FooTestDataFixture
    public class AzureDevOpsDeclare
    {
        public AdoWikiWithStorage AdoWikiWithStorage(
            ITimeline timeline,
            IFileSystem fs,
            IEnvironment env,
            string adoWikiUri,
            string adoPatEnvVar,
            string storageDirPath,
            int? pageViewsForDaysMax = null)
        {
            var adoWiki         = new AdoWiki(adoWikiUri, adoPatEnvVar, env, timeline);
            var storageDir      = new Dir(fs, storageDirPath);
            var storage         = AdoWikiPagesStatsStorage(storageDir, timeline.UtcNow);
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