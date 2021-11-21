using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.Tests
{
    [Category("integration")]
    [TestFixture]
    public class WikiTableOfContentsIntegrationTests
    {
        // kj2 simplify / dedup DI in this test with wikitools Program logic.
        [Test]
        public void WritesTableOfContentsFromLocalWikiGitClone()
        {
            var timeline = new Timeline();
            var env      = new Environment();
            var fs       = new FileSystem();
            var cfg      = new Configuration(fs).Read<WikitoolsTestsCfg>();

            var repoPaths = fs.FileTree(cfg.TestGitRepoClonePath).Paths;
            var wikiPagesPaths = new AdoWikiPagesPaths(repoPaths);

            var decl = new AzureDevOpsDeclare();
            var wiki = decl.AdoWikiWithStorage(
                new AdoWiki(cfg.AzureDevOpsCfg.AdoWikiUri, cfg.AzureDevOpsCfg.AdoPatEnvVar, env, timeline),
                fs,
                cfg.TestStorageDirPath,
                timeline.UtcNow);
            // kj2 when the pagesStats input goes beyond what is stored on file system, no exception is thrown, which is not great.
            var pagesStats = wiki.PagesStats(90).Result;

            var toc = new WikiTableOfContents(wikiPagesPaths, Task.FromResult(pagesStats));

            // kja 2 introduce abstraction for writing to temporary output test dir. Something like:
            // var testFile = new TemporaryTestFile(cfg.TestStorageDirPath);
            // // Act
            // var lines = testFile.Write(writable /* : IWritableToText */)
            var outputFilePath = cfg.TestStorageDirPath + Path.DirectorySeparatorChar +
                                 nameof(WritesTableOfContentsFromLocalWikiGitClone) + ".txt";

            // kja 2 should use fs
            using (var outputFileWriter = File.CreateText(outputFilePath))
            {
                // Act
                toc.WriteAsync(outputFileWriter).Wait();

            }

            Console.Out.WriteLine($"Wrote to {outputFilePath}");

            // kja 2 should use fs
            var actualLines = File.ReadAllLines(outputFilePath);

            Assert.That(
                actualLines.Length, 
                Is.GreaterThanOrEqualTo(cfg.TestGitRepoExpectedPathsMinPageCount));

            // kja 2 add assertion over min views count from cfg. This will need to show how to read back the data from file.
        }
    }
}