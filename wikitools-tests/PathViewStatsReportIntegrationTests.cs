using NUnit.Framework;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests;

namespace Wikitools.Tests;

[Category("integration")]
[TestFixture]
public class PathViewStatsReportIntegrationTests
{
    [Test]
    public void WritesPathViewStatsReport()
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).Read<WikitoolsIntegrationTestsCfg>();
        var pagesViewsReport = GitPagesViewsReport(fs, cfg.WikitoolsCfg, cfg.AzureDevOpsCfg);
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(pagesViewsReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static PathViewStatsReport GitPagesViewsReport(
        IFileSystem fs,
        WikitoolsCfg cfg, AzureDevOpsCfg adoCfg)
    {
        var timeline = new Timeline();
        var env = new Environment();

        var wiki = new AdoWikiWithStorageDeclare().AdoWikiWithStorage(
            timeline,
            fs,
            env,
            // kj2 instead here we there should be cfg.AzureDevOpsCfg.AdoWikiUri,
            // and below cfg.AzureDevOpsCfg.AdoPatEnvVar,
            // but currently Configuration class doesn't support more than one level of nesting
            // of configs. Even worse, it just throws null.
            adoCfg.AdoWikiUri,
            adoCfg.AdoPatEnvVar,
            cfg.StorageDirPath);

        // kj2 this will trigger call to ADO API. Not good. Should be deferred until WriteAll by the caller.
        // I might need to fix all Tasks to AsyncLazy to make this work, or by using new Task() and then task.Start();
        // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-5.0#separating-task-creation-and-execution
        // Maybe source generators could help here. See [Cache] and [Memoize] use cases here:
        // https://github.com/dotnet/roslyn/issues/16160
        // 11/17/2021: Or maybe doing stuff like LINQ IEnumerable is enough? IEnumerable and related
        // collections are lazy after all.
        var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

        var pagesViewsReport = new PathViewStatsReport(timeline, pagesViewsStats, cfg.AdoWikiPageViewsForDays);

        return pagesViewsReport;
    }
}