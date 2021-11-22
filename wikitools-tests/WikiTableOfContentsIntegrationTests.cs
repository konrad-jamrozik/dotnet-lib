using System;
using System.IO;
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

        // kj2 simplify / dedup DI in this test with wikitools Program logic.
        [Test]
        public void WritesTableOfContentsFromLocalWikiGitClone()
        {
            var timeline = new Timeline();
            var env      = new Environment();
            var fs       = new FileSystem();
            var cfg      = new Configuration(fs).Read<WikitoolsIntegrationTestsCfg>();

            var pathsInRepo = fs.FileTree(cfg.WikitoolsCfg.GitRepoClonePath).Paths;
            var wikiPagesPaths = new AdoWikiPagesPaths(pathsInRepo);

            var decl = new AzureDevOpsDeclare();
            var wiki = decl.AdoWikiWithStorage(
                new AdoWiki(cfg.AzureDevOpsCfg.AdoWikiUri, cfg.AzureDevOpsCfg.AdoPatEnvVar, env, timeline),
                fs,
                cfg.WikitoolsCfg.StorageDirPath,
                timeline.UtcNow);
            // kj2 when the pagesStats input goes beyond what is stored on file system, no exception is thrown, which is not great.
            var pagesStats = wiki.PagesStats(PageViewsForDays).Result;

            var toc = new WikiTableOfContents(wikiPagesPaths, Task.FromResult(pagesStats));

            var testFile = new TestFile(new Dir(fs, cfg.TestStorageDirPath));

            // Act
            var actualLines = testFile.Write(toc);

            Assert.That(
                actualLines.Length, 
                Is.GreaterThanOrEqualTo(cfg.TestGitRepoExpectedPathsMinPageCount));

            // kja 2 add assertion over min views count from cfg. This will need to show how to read back the data from file.
        }
    }
}