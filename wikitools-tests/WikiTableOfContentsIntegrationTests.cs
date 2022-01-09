using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.AzureDevOps;
using Wikitools.Config;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.Tests;

[Category("integration")]
[TestFixture]
public class WikiTableOfContentsIntegrationTests
{
    private const int PageViewsForDays = 90;

    [Test]
    public void WritesWikiTableOfContents()
    {
        var timeline = new Timeline();
        var fs = new FileSystem();
        var cfg = new Configuration(fs).Load<IWikitoolsTestsCfg>();
        var toc = WikiTableOfContents(timeline, fs, cfg);
        var testFile = new TestFile(cfg.TestStorageDir(fs));

        // Act
        var readLines = testFile.Write(toc);

        // .Skip(2) to skip the page header and empty line after it.
        var actualLines = readLines.Skip(2).Select(line => new WikiTableOfContents.Line(line))
            .ToList();

        Assert.That(
            actualLines.Count,
            Is.GreaterThanOrEqualTo(cfg.TestGitRepoExpectedPathsMinPageCount()));

        Assert.That(
            actualLines.Sum(line => line.Views),
            Is.GreaterThanOrEqualTo(cfg.TestGitRepoExpectedPathsMinPageViewsCount()));
    }

    private static WikiTableOfContents WikiTableOfContents(
        ITimeline timeline,
        IFileSystem fs,
        IWikitoolsTestsCfg cfg)
    {
        var wikiPagesPaths = AdoWikiPagesPaths(fs, cfg.WikitoolsCfg());
        var pagesStats = ValidWikiPagesStats(fs, cfg);
        var toc = new WikiTableOfContents(timeline, wikiPagesPaths, pagesStats);
        return toc;
    }

    private static AdoWikiPagesPaths AdoWikiPagesPaths(
        IFileSystem fs,
        IWikitoolsCfg cfg)
    {
        var pathsInRepo = fs.FileTree(cfg.GitRepoClonePath()).Paths;
        var wikiPagesPaths = new AdoWikiPagesPaths(pathsInRepo);
        return wikiPagesPaths;
    }

    private static async Task<ValidWikiPagesStats> ValidWikiPagesStats(
        IFileSystem fs,
        IWikitoolsTestsCfg cfg)
    {
        var timeline = new Timeline();
        var env      = new Environment();
        var wikiDecl = new AdoWikiWithStorageDeclare();
        var wiki = wikiDecl.AdoWikiWithStorage(
            timeline,
            fs,
            env,
            cfg.AzureDevOpsCfg.AdoWikiUri(),
            cfg.AzureDevOpsCfg.AdoPatEnvVar(),
            cfg.WikitoolsCfg().StorageDirPath());

        // kj2 when the pagesStats input goes beyond what is stored on file system, no exception is thrown, which is not great.
        var pagesStats = await wiki.PagesStats(PageViewsForDays);
        return pagesStats;
    }
}