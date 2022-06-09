using System.Linq;
using System.Threading.Tasks;
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
public class GitFilesStatsReportIntegrationTests
{
    [Test]
    public async Task WritesFilesStatsReport()
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).Load<IWikitoolsTestsCfg>();
        var filesReport = await GitFilesStatsReport(fs, cfg.WikitoolsCfg());
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(filesReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static async Task<GitFilesStatsReport> GitFilesStatsReport(
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

        var stats = await GitFileStats.From(gitLog, cfg.GitLogDays(), cfg.ExcludedPaths());

        var filesReport = new GitFilesStatsReport(
            timeline,
            cfg.GitLogDays(),
            stats);

        return filesReport;
    }
}