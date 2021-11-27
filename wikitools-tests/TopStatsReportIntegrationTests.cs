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
        int gitLogDays = 30; // kja change everywhere 30 to 28 days.
        var commits = GitLogCommits(fs, cfg, gitLogDays);
        // kja add filter to 7 Days
        var pagesStatsLast7Days = PagesStats(timeline, fs, cfg, adoCfg);

        var ago1Day = timeline.DaysFromUtcNow(-1).AddDays(-5); // kja temp. And -5 below.
        var ago7Days = timeline.DaysFromUtcNow(-7).AddDays(-5);
        var ago30Days = timeline.DaysFromUtcNow(-30);
        
        var commitsLast7Days = GitLogCommit.FilterCommits(commits, (ago7Days, ago1Day));
        var commitsLast30Days = GitLogCommit.FilterCommits(commits, (ago30Days, ago1Day));
        
        var authorStatsLast7Days = GitAuthorStats(cfg, commitsLast7Days, top: 5);
        var authorStatsLast30Days = GitAuthorStats(cfg, commitsLast30Days, top: 10);
        var fileStatsLast7Days = GitFileStats(cfg, commitsLast7Days, top: 10);
        var fileStatsLast30Days = GitFileStats(cfg, commitsLast30Days, top: 20);
        var pageViewStatsLast7Days = PageViewStats(pagesStatsLast7Days, top: 10);

        var authorsReport = new TopStatsReport(
            timeline,
            cfg.AdoWikiPageViewsForDays,
            authorStatsLast7Days, 
            authorStatsLast30Days,
            fileStatsLast7Days,
            fileStatsLast30Days,
            pageViewStatsLast7Days);

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
        int top)
    {
        bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
        var authorStats = Wikitools.GitAuthorStats.From(commits, AuthorFilter, top);
        return authorStats;
    }

    private static GitFileStats[] GitFileStats(WikitoolsCfg cfg, GitLogCommit[] commits, int top)
    {
        bool FilePathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);
        var fileStats = Wikitools.GitFileStats.From(commits, FilePathFilter, top);
        return fileStats;
    }

    private static PathViewStats[] PageViewStats(
        ValidWikiPagesStats pagesStats,
        int top)
    {
        var pageViewStats = PathViewStats.From(pagesStats, top);
        return pageViewStats;
    }

    private static ValidWikiPagesStats PagesStats(
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
        var pagesStatsResult = pagesStats.Result; // kj2 .Result
        return pagesStatsResult;
    }
}