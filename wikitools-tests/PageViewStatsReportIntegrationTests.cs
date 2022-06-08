using NUnit.Framework;
using System.Linq;
using Wikitools.AzureDevOps;
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
        var cfg = new Configuration(fs).Load<IWikitoolsTestsCfg>();
        var pagesViewsReport = GitPagesViewsReport(fs, cfg.WikitoolsCfg());
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(pagesViewsReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static PageViewStatsReport GitPagesViewsReport(
        IFileSystem fs,
        IWikitoolsCfg cfg)
    {
        var timeline = new Timeline();
        var env = new Environment();

        var wiki = new AdoWikiWithStorageDeclare().AdoWikiWithStorage(
            fs,
            env,
            cfg.AzureDevOpsCfg().AdoWikiUri(),
            cfg.AzureDevOpsCfg().AdoPatEnvVar(),
            cfg.StorageDirPath());

        int daysAgo = 30 * 10;

        var pagesViewsReport = new PageViewStatsReport(
            timeline,
            wiki,
            daysAgo);

        return pagesViewsReport;
    }
}