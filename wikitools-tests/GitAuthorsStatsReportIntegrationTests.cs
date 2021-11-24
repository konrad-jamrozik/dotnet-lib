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
public class GitAuthorsStatsReportIntegrationTests
{
    [Test]
    public void WritesAuthorsStatsReport()
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).Read<WikitoolsIntegrationTestsCfg>();
        var authorsReport = GitAuthorsStatsReport(fs, cfg.WikitoolsCfg);
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(authorsReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static GitAuthorsStatsReport GitAuthorsStatsReport(
        IFileSystem fs,
        WikitoolsCfg cfg)
    {
        var timeline = new Timeline();
        var os = new WindowsOS();
        bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);

        var gitLogDecl = new GitLogDeclare();
        var gitLog = gitLogDecl.GitLog(
            os,
            cfg.GitRepoCloneDir(fs),
            cfg.GitExecutablePath);
        var recentCommits = gitLog.Commits(cfg.GitLogDays);
        var authorsReport = new GitAuthorsStatsReport(
            timeline,
            recentCommits,
            cfg.GitLogDays,
            cfg.Top,
            AuthorFilter);

        return authorsReport;
    }
}