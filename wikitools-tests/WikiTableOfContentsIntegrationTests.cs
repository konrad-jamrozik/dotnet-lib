using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.Tests
{
    [Category("integration")]
    [TestFixture]
    public class WikiTableOfContentsIntegrationTests
    {
        private const int PageViewsForDays = 90;

        [Test]
        public void WritesWikiTableOfContents()
        {
            var fs = new FileSystem();
            var cfg = new Configuration(fs).Read<WikitoolsIntegrationTestsCfg>();
            var toc = WikiTableOfContents(fs, cfg);
            var testFile = new TestFile(cfg.TestStorageDir(fs));

            // Act
            var readLines = testFile.Write(toc);

            var actualLines = readLines.Select(line => new WikiTableOfContents.Line(line)).ToList();

            Assert.That(
                actualLines.Count,
                Is.GreaterThanOrEqualTo(cfg.TestGitRepoExpectedPathsMinPageCount));

            Assert.That(
                actualLines.Sum(line => line.Views),
                Is.GreaterThanOrEqualTo(cfg.TestGitRepoExpectedPathsMinPageViewsCount));
        }

        private static WikiTableOfContents WikiTableOfContents(
            IFileSystem fs,
            WikitoolsIntegrationTestsCfg cfg)
        {
            var wikiPagesPaths = AdoWikiPagesPaths(fs, cfg.WikitoolsCfg);
            var pagesStats = ValidWikiPagesStats(fs, cfg);
            var toc = new WikiTableOfContents(wikiPagesPaths, Task.FromResult(pagesStats));
            return toc;
        }

        private static AdoWikiPagesPaths AdoWikiPagesPaths(
            IFileSystem fs,
            WikitoolsCfg cfg)
        {
            var pathsInRepo = fs.FileTree(cfg.GitRepoClonePath).Paths;
            var wikiPagesPaths = new AdoWikiPagesPaths(pathsInRepo);
            return wikiPagesPaths;
        }

        private static ValidWikiPagesStats ValidWikiPagesStats(
            IFileSystem fs,
            WikitoolsIntegrationTestsCfg cfg)
        {
            var timeline = new Timeline();
            var env      = new Environment();
            var wikiDecl = new AdoWikiWithStorageDeclare();
            var wiki = wikiDecl.AdoWikiWithStorage(
                timeline,
                fs,
                env,
                cfg.AzureDevOpsCfg.AdoWikiUri,
                cfg.AzureDevOpsCfg.AdoPatEnvVar,
                cfg.WikitoolsCfg.StorageDirPath);

            // kj2 when the pagesStats input goes beyond what is stored on file system, no exception is thrown, which is not great.
            // kj2 get rid of all .Result and .Wait() calls in the codebase, if possible.
            var pagesStats = wiki.PagesStats(PageViewsForDays).Result;
            return pagesStats;
        }
    }
}