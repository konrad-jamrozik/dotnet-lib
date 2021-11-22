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
        public void WritesTableOfContentsFromLocalWikiGitClone()
        {
            var fs = new FileSystem();
            var cfg = new Configuration(fs).Read<WikitoolsIntegrationTestsCfg>();
            var toc = WikiTableOfContents(fs, cfg);
            var testFile = new TestFile(cfg.TestStorageDir(fs));

            // Act
            var actualLines = testFile.Write(toc);

            Assert.That(
                actualLines.Length, 
                Is.GreaterThanOrEqualTo(cfg.TestGitRepoExpectedPathsMinPageCount));

            // kja 2 add assertion over min views count from cfg. This will need to show how to read back the data from file.
        }

        private static WikiTableOfContents WikiTableOfContents(
            FileSystem fs,
            WikitoolsIntegrationTestsCfg cfg)
        {
            var wikiPagesPaths = AdoWikiPagesPaths(fs, cfg);
            var pagesStats = ValidWikiPagesStats(fs, cfg);
            var toc = new WikiTableOfContents(wikiPagesPaths, Task.FromResult(pagesStats));
            return toc;
        }

        private static AdoWikiPagesPaths AdoWikiPagesPaths(
            IFileSystem fs,
            WikitoolsIntegrationTestsCfg cfg)
        {
            var pathsInRepo = fs.FileTree(cfg.WikitoolsCfg.GitRepoClonePath).Paths;
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
            var pagesStats = wiki.PagesStats(PageViewsForDays).Result;
            return pagesStats;
        }
    }
}