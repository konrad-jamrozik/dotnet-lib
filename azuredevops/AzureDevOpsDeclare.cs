using System;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    // kj2 Other possible suffixes: Ctor, Def, Scaffolding, Blueprint, Chart (a'la Helm Chart), Factory, DI (for Dep. Inj).
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
            var storageDecl     = new AdoWikiPagesStatsStorageDeclare();
            var storage         = storageDecl.AdoWikiPagesStatsStorage(storageDir, timeline.UtcNow);
            var wikiWithStorage = AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysMax);
            return wikiWithStorage;
        }

        public AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            AdoWikiPagesStatsStorage storage,
            int? pageViewsForDaysMax = null) 
            => new AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysMax);
    }
}