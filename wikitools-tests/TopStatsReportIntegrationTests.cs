using System.Linq;
using NUnit.Framework;
using Wikitools.AzureDevOps;
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
        var cfg = new Configuration(fs).Read<WikitoolsIntegrationTestsCfg>();
        var topStatsReport = TopStatsReport(fs, cfg.WikitoolsCfg, cfg.AzureDevOpsCfg);
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(topStatsReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static TopStatsReport TopStatsReport(
        IFileSystem fs,
        WikitoolsCfg cfg,
        AzureDevOpsCfg adoCfg)
    {
        // kja dedup logic used in this method, and in int tests for other reports using the same data.

        var timeline = new Timeline();
        var os = new WindowsOS();

        var gitLog = new GitLogDeclare().GitLog(
            os,
            cfg.GitRepoCloneDir(fs),
            cfg.GitExecutablePath);

        var commits = gitLog.Commits(cfg.GitLogDays);
        var commitsResult = commits.Result; // kj2 .Result
        bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
        bool FilePathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);
        
        var authorStats = GitAuthorStats.From(commitsResult, AuthorFilter, cfg.Top); 
        var fileStats = GitFileStats.From(commitsResult, FilePathFilter, cfg.Top);

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

        var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

        // kj2 .Result
        var pathViewStats = PathViewStats.From(pagesViewsStats.Result);

        var authorsReport = new TopStatsReport(
            timeline,
            cfg.GitLogDays,
            cfg.AdoWikiPageViewsForDays,
            authorStats, 
            fileStats,
            pathViewStats);

        return authorsReport;
    }
}