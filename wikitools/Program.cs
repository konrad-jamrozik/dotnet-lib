using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            ITimeline timeline = new Timeline();
            IOperatingSystem os = new WindowsOS();
            IFileSystem fs = new FileSystem();
            IEnvironment env = new Environment();

            var docsToWrite = DocsToWrite(timeline, os, fs, env);
            var outputSink  = Console.Out;
            await WriteAll(docsToWrite, outputSink);
        }

        private static MarkdownDocument[] DocsToWrite(
            ITimeline timeline,
            IOperatingSystem os,
            IFileSystem fs,
            IEnvironment env)
        {
            var cfg = new Configuration(fs).Read<WikitoolsCfg>();
            var wikiDecl = new AdoWikiWithStorageDeclare();
            var wiki = wikiDecl.AdoWikiWithStorage(
                timeline,
                fs,
                env,
                cfg.AzureDevOpsCfg.AdoWikiUri,
                cfg.AzureDevOpsCfg.AdoPatEnvVar,
                cfg.StorageDirPath);

            var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            var wikiToc = new WikiTableOfContents(
                new AdoWikiPagesPaths(fs.FileTree(cfg.GitRepoClonePath).Paths),
                pagesViewsStats);

            var docsToWrite = new MarkdownDocument[]
            {
                wikiToc
            };
            return docsToWrite;
        }

        private static Task WriteAll(MarkdownDocument[] docs, TextWriter textWriter) =>
            Task.WhenAll(docs.Select(doc => doc.WriteAsync(textWriter)).ToArray());
    }
}

// kja 1 high-level todos
// 1. One page TOC
// - Annotations (icons): Newly added, lots of traffic, stale.
/*
 * Work progress milestones:
 * 11/21/2021:
 * - Writing out one wiki page TOC data, with stats from storage (>30 days).
 * 2/27/2021:
 * - Persisting wiki data to HDD as monthly json, allowing to report more than 30 days.
 * - Tooling for merging and splitting existing wiki data.
 * - To review these changes, explore from: Program, AdoWikiWithStorage
 * 1/18/2021:
 * - Markdown reports with data pulled from git.
 * - Markdown report with page view stats data pulled from ADO API.
 * - To review these changes, explore from: Program, GitAuthorsStatsReport, GitFilesStatsReport, MonthlyStatsReport, PagesViewsStatsReport
 * 12/29/2020:
 * - Work start. Reused GitRepository and AsyncLazy abstractions written in 2017.
 */
