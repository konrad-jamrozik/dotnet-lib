using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
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

    private static GitAuthorsStatsReport TopStatsReport(
        IFileSystem fs,
        WikitoolsCfg cfg)
    {
        var timeline = new Timeline();
        var os = new WindowsOS();

        var gitLog = new GitLogDeclare().GitLog(
            os,
            cfg.GitRepoCloneDir(fs),
            cfg.GitExecutablePath);

        // kja current work: introduce TopStatsReport.
        Task<GitLogCommit[]> commits = gitLog.Commits(cfg.GitLogDays);
        int? top = cfg.Top;
        Func<string, bool>? authorFilter = author => !cfg.ExcludedAuthors.Any(author.Contains);
        var authorsReport = new GitAuthorsStatsReport(
            timeline,
            cfg.GitLogDays,
            GitAuthorStats.AuthorsStatsFrom(commits.Result, authorFilter ?? (_ => true), top));

        return authorsReport;
    }
}