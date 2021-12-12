using System.Linq;
using NUnit.Framework;
using Wikitools.Lib;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests;

namespace Wikitools.Tests;

[Category("integration")]
[TestFixture]
public class GitFilesStatsReportIntegrationTests
{
    [Test]
    public void WritesFilesStatsReport()
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).Read<WikitoolsIntegrationTestsCfg>();
        var filesReport = GitFilesStatsReport(fs, cfg.WikitoolsCfg);
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var lines = testFile.Write(filesReport);

        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(lines.Count(l => l.StartsWith("| ")), Is.GreaterThanOrEqualTo(3));
    }

    private static GitFilesStatsReport GitFilesStatsReport(
        IFileSystem fs,
        WikitoolsCfg cfg)
    {
        var timeline = new Timeline();
        var os = new WindowsOS();

        Dir gitRepoDir = cfg.GitRepoCloneDir(fs);
        var gitLog = new GitLogDeclare().GitLog(
            timeline,
            os,
            gitRepoDir,
            cfg.GitExecutablePath);

        var commits = gitLog.Commits(cfg.GitLogDays);
        bool FilePathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);
        // kj2 result
        var stats = GitFileStats.From(commits.Result, FilePathFilter, cfg.Top);

        var filesReport = new GitFilesStatsReport(
            timeline,
            cfg.GitLogDays,
            stats);

        return filesReport;
    }
}