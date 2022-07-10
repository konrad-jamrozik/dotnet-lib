using System.Linq;
using NUnit.Framework;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Config;
using Wikitools.Config;
using Wikitools.Lib;
using Wikitools.Lib.Configuration;
using Wikitools.Lib.Git;
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
        var cfg = new Configuration(fs).Load<IWikitoolsTestsCfg>();
        var topStatsReport = TopStatsReport(
            fs,
            cfg.WikitoolsCfg(),
            cfg.AzureDevOpsCfg);
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
        // kj2-testdata dedup logic used in this method, and in int tests for other reports using the same data.
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
            cfg.GitExecutablePath());
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
            fs,
            env,
            adoCfg.AdoWikiUri(),
            adoCfg.AdoPatEnvVar(),
            cfg.StorageDirPath()); // kj2 here int test is using PROD storage dir instead of
        // Wikitools.AzureDevOps.Config.IAzureDevOpsTestsCfg.TestStorageDirPath
        // because it requires real data. But it is confusing - one would expect for the data
        // to be in a test storage dir, not prod storage dir. Need to ponder what to do about it.
        // Maybe I need a new dir, "int test storage dir". The test then would
        // copy-over the required data, and only that data, to it?
        // But the problem is I actually need production data - I am in fact using 
        // these int tests as normal tool usage. So really proper fix is to convert it 
        // from a int test, to a normal cmd line tool.
        return wiki;
    }
}