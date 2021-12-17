using System.Linq;
using NUnit.Framework;
using Wikitools.AzureDevOps;
using Wikitools.Lib;
using Wikitools.Lib.Data;
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

        var timeline = new Timeline();
        var dataDays = 28;
        var ago1Day = timeline.DaysFromUtcNow(-1);
        var ago7Days = timeline.DaysFromUtcNow(-7);
        var ago28Days = timeline.DaysFromUtcNow(-dataDays);
        var last7Days = new DaySpan(ago7Days, ago1Day);
        var last28Days = new DaySpan(ago28Days, ago1Day);

        var gitLog = GitLog(timeline, fs, cfg, dataDays);
        var commits = gitLog.Commits(dataDays).Result; // kj2 .Result
        
        var commitsLast7Days = new GitLogCommits(commits, last7Days);
        var commitsLast28Days = new GitLogCommits(commits, last28Days);
        
        var authorStatsLast7Days = GitAuthorStats(cfg, commitsLast7Days, top: 3);
        var authorStatsLast28Days = GitAuthorStats(cfg, commitsLast28Days, top: 5);
        var fileStatsLast7Days = GitFileStats(cfg, commitsLast7Days, top: 5);
        var fileStatsLast28Days = GitFileStats(cfg, commitsLast28Days, top: 10);

        // Here, The 1 is added to dataDays for pageViewsForDays
        // to account for how ADO REST API interprets the range.
        // For more, see comment on:
        // AdoWikiWithStorageIntegrationTests.ObtainsAndStoresDataFromAdoWikiForToday
        var pagesStats = PagesStats(timeline, fs, cfg, adoCfg, pageViewsForDays: dataDays + 1);
        var pagesStatsLast7Days = PageViewStats(pagesStats.Trim(ago7Days, ago1Day), top: 5);
        var pagesStatsLast28Days = PageViewStats(pagesStats.Trim(ago28Days, ago1Day), top: 10);

        var topStatsReport = new TopStatsReport(
            timeline,
            authorStatsLast7Days, 
            authorStatsLast28Days,
            fileStatsLast7Days,
            fileStatsLast28Days,
            pagesStatsLast7Days,
            pagesStatsLast28Days);

        // kja curr work
        var newTopStatsReport = new TopStatsReport(timeline, gitLog, cfg.ExcludedAuthors);

        return topStatsReport;
    }

    private static GitLog GitLog(
        ITimeline timeline,
        IFileSystem fs,
        WikitoolsCfg cfg,
        int gitLogDays)
    {
        var os = new WindowsOS();

        Dir gitRepoDir = cfg.GitRepoCloneDir(fs);
        var gitLog = new GitLogDeclare().GitLog(
            timeline,
            os,
            gitRepoDir,
            cfg.GitExecutablePath);
        return gitLog;
    }

    private static RankedTop<GitAuthorStats> GitAuthorStats(
        WikitoolsCfg cfg,
        GitLogCommits commits,
        int top)
    {
        bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
        var authorStats = Wikitools.GitAuthorStats.From(commits, AuthorFilter, top, addIcons: true);
        return authorStats;
    }

    private static GitFileStats[] GitFileStats(WikitoolsCfg cfg, GitLogCommits commits, int top)
    {
        bool FilePathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);
        var fileStats = Wikitools.GitFileStats.From(commits, FilePathFilter, top);
        return fileStats;
    }

    private static PageViewStats[] PageViewStats(
        ValidWikiPagesStats pagesStats,
        int top)
    {
        var pageViewStats = Wikitools.PageViewStats.From(pagesStats, top);
        return pageViewStats;
    }

    private static ValidWikiPagesStats PagesStats(
        ITimeline timeline,
        IFileSystem fs,
        WikitoolsCfg cfg,
        AzureDevOpsCfg adoCfg,
        int pageViewsForDays)
    {
        var env = new Environment();

        var wiki = new AdoWikiWithStorageDeclare().AdoWikiWithStorage(
            timeline,
            fs,
            env,
            adoCfg.AdoWikiUri,
            adoCfg.AdoPatEnvVar,
            cfg.StorageDirPath);

        var pagesStats = wiki.PagesStats(pageViewsForDays);
        var pagesStatsResult = pagesStats.Result; // kj2 .Result
        return pagesStatsResult;
    }
}