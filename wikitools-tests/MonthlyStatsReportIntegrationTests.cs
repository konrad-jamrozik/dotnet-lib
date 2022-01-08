using System.Linq;
using NUnit.Framework;
using Wikitools.Config;
using Wikitools.Lib;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
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
        var cfg = new Configuration(fs).ReadFromAssembly<IWikitoolsIntegrationTestsCfg>();
        var filesReport = GitMonthlyStatsReport(fs, cfg.WikitoolsCfg());
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(filesReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static MonthlyStatsReport GitMonthlyStatsReport(
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

        var monthlyReport = new MonthlyStatsReport(
            gitLog,
            // kja make cfg have DaySpan instead
            new DaySpan(cfg.MonthlyReportStartDay(), cfg.MonthlyReportEndDay()),
            cfg.ExcludedAuthors(),
            cfg.ExcludedPaths());

        return monthlyReport;
    }
}