using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;
// kj2 Other possible suffixes for Declare (update README.md):
// Ctor, Def, Scaffolding, Blueprint, Recipe, Chart (a'la Helm Chart), Factory, DI (for Dep. Inj).
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
        string wikiUri,
        string adoPatEnvVar,
        string storageDirPath)
    {
        var wiki        = new AdoWiki(wikiUri, adoPatEnvVar, env);
        var storageDir  = new Dir(fs, storageDirPath);
        var storageDecl = new AdoWikiPagesStatsStorageDeclare();
        var storage     = storageDecl.New(storageDir);
        var wikiWithStorage = AdoWikiWithStorage(wiki, storage);
        return wikiWithStorage;
    }

    public AdoWikiWithStorage AdoWikiWithStorage(
        IAdoWiki wiki,
        AdoWikiPagesStatsStorage storage,
        int? pageViewsForDaysMax = null) 
        => new AdoWikiWithStorage(wiki, storage, pageViewsForDaysMax);
}