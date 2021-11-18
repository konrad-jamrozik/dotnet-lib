using System;
using System.Collections.Generic;
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
            var cfg      = new Configuration(fs).Read<WikitoolsCfg>();

            var repoPaths = fs.FileTree(cfg.GitRepoClonePath).Paths;
            var wikiPagesPaths = new AdoWikiPagesPaths(repoPaths);

            var decl = new AzureDevOpsDeclare();
            var wiki = decl.AdoWikiWithStorage(
                new AdoWiki(cfg.AzureDevOpsCfg.AdoWikiUri, cfg.AzureDevOpsCfg.AdoPatEnvVar, env, timeline),
                fs,
                cfg.StorageDirPath,
                timeline.UtcNow);
            // kja when the pagesStats input goes beyond what is stored on file system, no exception is thrown, which is not great.
            var pagesStats = wiki.PagesStats(90).Result;

            var toc = new WikiTableOfContents(wikiPagesPaths, Task.FromResult(pagesStats));

            // Act
            var contentResult = toc.Content.Result;

            for (var i = 0; i < contentResult.Length; i++)
            {
                // kja 5 make proper test assertion. Probably some count based on config file.
                Console.Out.WriteLine(contentResult[i] + "  ");
            }
        }
    }
}