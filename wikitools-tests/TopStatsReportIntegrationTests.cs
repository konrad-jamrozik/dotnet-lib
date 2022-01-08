using System.Linq;
using NUnit.Framework;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Config;
using Wikitools.Config;
using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.Tests;

[Category("integration")]
[TestFixture]
public class TopStatsReportIntegrationTests
{
    [Test]
    public void WritesTopStatsReport()
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).ReadFromAssembly<IWikitoolsIntegrationTestsCfg>();
        var topStatsReport = TopStatsReport(fs, cfg.WikitoolsCfg(), cfg.AzureDevOpsCfg());
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(topStatsReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static TopStatsReport TopStatsReport(
        IFileSystem fs,
        IWikitoolsCfg cfg,
        IAzureDevOpsCfg adoCfg)
    {
        // kj2 dedup logic used in this method, and in int tests for other reports using the same data.
        var timeline = new Timeline();
        var gitLog = GitLog(timeline, fs, cfg);
        var wiki = AdoWiki(timeline, fs, cfg, adoCfg);
        var topStatsReport = new TopStatsReport(
             timeline,
             gitLog,
             wiki,
             cfg.ExcludedAuthors(),
             cfg.ExcludedPaths());

        return topStatsReport;
    }

    private static GitLog GitLog(
        ITimeline timeline,
        IFileSystem fs,
        IWikitoolsCfg cfg)
    {
        var os = new WindowsOS();
        Dir gitRepoDir = cfg.GitRepoCloneDir(fs);
        var gitLog = new GitLogDeclare().GitLog(
            timeline,
            os,
            gitRepoDir,
            cfg.GitExecutablePath()); // kja pass cfg.GitExecutablePath() as arg
        return gitLog;
    }

    private static AdoWikiWithStorage AdoWiki(
        ITimeline timeline,
        IFileSystem fs,
        IWikitoolsCfg cfg,
        IAzureDevOpsCfg adoCfg)
    {
        var env = new Environment();
        var wiki = new AdoWikiWithStorageDeclare().AdoWikiWithStorage(
            timeline,
            fs,
            env,
            adoCfg.AdoWikiUri(),
            adoCfg.AdoPatEnvVar(),
            cfg.StorageDirPath());
        return wiki;
    }
}