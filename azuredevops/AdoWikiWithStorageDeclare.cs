using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    // kj2 Other possible suffixes: Ctor, Def, Scaffolding, Blueprint, Chart (a'la Helm Chart), Factory, DI (for Dep. Inj).
    //
    // From https://helm.sh/docs/topics/architecture/
    // "The chart is a bundle of information necessary to create an instance of a Kubernetes application."
    // "The config contains configuration information that can be merged into a packaged chart to create a releasable object."
    //
    // With that, for each class Foo, we will have:
    // Foo, FooChart, FooTests, FooIntegrationTests, FooFixture, FooTestData, FooTestDataFixture
    
    public class AdoWikiWithStorageDeclare
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
            var adoWiki     = new AdoWiki(adoWikiUri, adoPatEnvVar, env, timeline);
            var storageDir  = new Dir(fs, storageDirPath);
            var storageDecl = new AdoWikiPagesStatsStorageDeclare();
            var storage     = storageDecl.AdoWikiPagesStatsStorage(storageDir, timeline.UtcNow);
            var wiki        = AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysMax);
            return wiki;
        }

        public AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            AdoWikiPagesStatsStorage storage,
            int? pageViewsForDaysMax = null) 
            => new AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysMax);
    }
}