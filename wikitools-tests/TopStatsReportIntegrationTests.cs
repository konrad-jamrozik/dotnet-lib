using System;
using System.Linq;
using NUnit.Framework;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests;

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
        var topStatsReport = TopStatsReport(fs, cfg.WikitoolsCfg);
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(topStatsReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static TopStatsReport TopStatsReport(
        IFileSystem fs,
        WikitoolsCfg cfg)
    {
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
        
        var authorStats = GitAuthorStats.GitAuthorsStatsFrom(commitsResult, AuthorFilter, cfg.Top); 
        var fileStats = GitFileStats.GitFilesStatsFrom(commitsResult, FilePathFilter, cfg.Top);

        var authorsReport = new TopStatsReport(
            timeline,
            cfg.GitLogDays,
            authorStats, 
            fileStats);

        return authorsReport;
    }
}