using NUnit.Framework;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Config;
using Wikitools.Config;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests;

namespace Wikitools.Tests;

[Category("integration")]
[TestFixture]
public class PageViewStatsReportIntegrationTests
{
    [Test]
    public void WritesPageViewStatsReport()
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).ReadFromAssembly<IWikitoolsIntegrationTestsCfg>();
        var pagesViewsReport = GitPagesViewsReport(fs, cfg.WikitoolsCfg(), cfg.AzureDevOpsCfg());
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(pagesViewsReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static PageViewStatsReport GitPagesViewsReport(
        IFileSystem fs,
        IWikitoolsCfg cfg, IAzureDevOpsCfg adoCfg)
    {
        var timeline = new Timeline();
        var env = new Environment();

        var wiki = new AdoWikiWithStorageDeclare().AdoWikiWithStorage(
            timeline,
            fs,
            env,
            // kja instead here there should be cfg.AzureDevOpsCfg.AdoWikiUri,
            // and below cfg.AzureDevOpsCfg.AdoPatEnvVar,
            // but currently (<- no longer true, after migration to C# private repo)
            // Configuration class doesn't support more than one level of nesting
            // of configs. Even worse, it just throws null.
            adoCfg.AdoWikiUri(),
            adoCfg.AdoPatEnvVar(),
            cfg.StorageDirPath());

        int pageViewsForDays = 30 * 10;

        var pagesViewsReport = new PageViewStatsReport(
            timeline,
            wiki,
            pageViewsForDays);

        return pagesViewsReport;
    }
}