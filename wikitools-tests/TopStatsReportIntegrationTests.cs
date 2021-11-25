using System.Linq;
using NUnit.Framework;
using Wikitools.AzureDevOps;
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
        // kj2 dedup logic used in this method, and in int tests for other reports using the same data.

        // kja current work - see comment on Wikitools.TopStatsReport

        var timeline = new Timeline();
        int gitLogDays = 30;
        var commits = GitLogCommits(fs, cfg, gitLogDays);
        var ago30Days = new DateDay(timeline.UtcNow.AddDays(-30));
        var ago7Days = new DateDay(timeline.UtcNow.AddDays(-7));
        var ago1Day = new DateDay(timeline.UtcNow.AddDays(-1));
        var authorLastWeekStats = GitAuthorStats(cfg, commits, top: 5, daySpan: (ago7Days, ago1Day));
        var authorLast30DaysStats = GitAuthorStats(cfg, commits, top: 10, daySpan: (ago30Days, ago1Day));
        var fileStats = GitFileStats(cfg, commits, top: 10);
        var pageViewStats = PageViewStats(timeline, fs, cfg, adoCfg);
        var authorsReport = new TopStatsReport(
            timeline,
            gitLogDays,
            cfg.AdoWikiPageViewsForDays,
            authorLastWeekStats, 
            authorLast30DaysStats,
            fileStats,
            pageViewStats);

        return authorsReport;
    }

    private static GitLogCommit[] GitLogCommits(IFileSystem fs, WikitoolsCfg cfg, int gitLogDays)
    {
        var os = new WindowsOS();

        var gitLog = new GitLogDeclare().GitLog(
            os,
            cfg.GitRepoCloneDir(fs),
            cfg.GitExecutablePath);

        var commits = gitLog.Commits(gitLogDays);
        var commitsResult = commits.Result; // kj2 .Result
        return commitsResult;
    }

    private static GitAuthorStats[] GitAuthorStats(
        WikitoolsCfg cfg,
        GitLogCommit[] commits,
        int top,
        (DateDay sinceDay, DateDay untilDay) daySpan)
    {
        bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
        var authorStats = Wikitools.GitAuthorStats.From(commits, AuthorFilter, top, daySpan);
        return authorStats;
    }

    private static GitFileStats[] GitFileStats(WikitoolsCfg cfg, GitLogCommit[] commits, int top)
    {
        bool FilePathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);
        var fileStats = Wikitools.GitFileStats.From(commits, FilePathFilter, top);
        return fileStats;
    }

    private static PathViewStats[] PageViewStats(
        ITimeline timeline,
        IFileSystem fs,
        WikitoolsCfg cfg,
        AzureDevOpsCfg adoCfg)
    {
        var env = new Environment();

        var wiki = new AdoWikiWithStorageDeclare().AdoWikiWithStorage(
            timeline,
            fs,
            env,
            adoCfg.AdoWikiUri,
            adoCfg.AdoPatEnvVar,
            cfg.StorageDirPath);

        var pagesStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

        // kj2 .Result
        var pageViewStats = PathViewStats.From(pagesStats.Result);
        return pageViewStats;
    }
}