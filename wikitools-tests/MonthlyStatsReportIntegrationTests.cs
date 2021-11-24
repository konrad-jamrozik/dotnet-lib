using System.Linq;
using NUnit.Framework;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Tests;

namespace Wikitools.Tests;

[Category("integration")]
[TestFixture]
public class MonthlyStatsReportIntegrationTests
{
    [Test]
    public void WritesMonthlyStatsReport()
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).Read<WikitoolsIntegrationTestsCfg>();
        var filesReport = GitMonthlyStatsReport(fs, cfg.WikitoolsCfg);
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(filesReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static MonthlyStatsReport GitMonthlyStatsReport(
        IFileSystem fs,
        WikitoolsCfg cfg)
    {
        var os = new WindowsOS();
        var gitLogDecl = new GitLogDeclare();
        var gitLog = gitLogDecl.GitLog(
            os,
            cfg.GitRepoCloneDir(fs),
            cfg.GitExecutablePath);
        var pastCommits = gitLog.Commits(cfg.MonthlyReportStartDate, cfg.MonthlyReportEndDate);
        bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
        bool PathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);

        var monthlyReport = new MonthlyStatsReport(pastCommits, AuthorFilter, PathFilter);

        return monthlyReport;
    }
}