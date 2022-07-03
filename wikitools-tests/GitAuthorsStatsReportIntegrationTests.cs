using System.Linq;
using NUnit.Framework;
using Wikitools.Config;
using Wikitools.Lib;
using Wikitools.Lib.Configuration;
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
        var cfg = new Configuration(fs).Load<IWikitoolsTestsCfg>();
        var authorsReport = GitAuthorsStatsReport(fs, cfg.WikitoolsCfg());
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(authorsReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static GitAuthorsStatsReport GitAuthorsStatsReport(
        IFileSystem fs,
        IWikitoolsCfg cfg)
    {
        var timeline = new Timeline();
        var os = new WindowsOS();

        Dir gitRepoDir = cfg.GitRepoCloneDir(fs);
        var gitLog = new GitLogDeclare().GitLog(
            timeline,
            os,
            gitRepoDir,
            cfg.GitExecutablePath());

        var authorsReport = new GitAuthorsStatsReport(
            gitLog,
            cfg.Top(),
            cfg.GitLogDays(),
            cfg.ExcludedAuthors());

        return authorsReport;
    }
}