﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
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
            ITimeline        timeline = new Timeline();
            IOperatingSystem os       = new WindowsOS();
            IFileSystem      fs       = new FileSystem();
            IEnvironment     env      = new Environment();
            WikitoolsCfg  cfg      = WikitoolsCfg.From(fs);
            IAdoWiki         adoWiki  = new AdoWiki(cfg.AzureDevOpsTestsCfg.AdoWikiUri, cfg.AzureDevOpsTestsCfg.AdoPatEnvVar, env);

            var docsToWrite = DocsToWrite(timeline, os, fs, adoWiki, cfg);
            var outputSink  = Console.Out;

            await WriteAll(docsToWrite, outputSink);
        }

        private static MarkdownDocument[] DocsToWrite(
            ITimeline timeline,
            IOperatingSystem os,
            IFileSystem fs,
            IAdoWiki adoWiki,
            WikitoolsCfg cfg)
        {
            var decl   = new Declare();
            var gitLog = decl.GitLog(os, new Dir(fs, cfg.GitRepoClonePath), cfg.GitExecutablePath);
            var wiki = decl.AdoWikiWithStorage(
                adoWiki,
                fs,
                cfg.StorageDirPath,
                timeline.UtcNow);

            var recentCommits = gitLog.Commits(cfg.GitLogDays);
            var pastCommits   = gitLog.Commits(cfg.MonthlyReportStartDate, cfg.MonthlyReportEndDate);

            // kj2 this will trigger call to ADO API. Not good. Should be deferred until WriteAll by the caller.
            // I might need to fix all Tasks to AsyncLazy to make this work, or by using new Task() and then task.Start();
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-5.0#separating-task-creation-and-execution
            // Maybe source generators could help here. See [Cache] and [Memoize] use cases here:
            // https://github.com/dotnet/roslyn/issues/16160
            var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
            bool PathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);

            var authorsReport    = new GitAuthorsStatsReport(timeline, recentCommits, cfg.GitLogDays, cfg.Top, AuthorFilter);
            var filesReport      = new GitFilesStatsReport(timeline, recentCommits, cfg.GitLogDays, cfg.Top, PathFilter);
            var pagesViewsReport = new PagesViewsStatsReport(timeline, pagesViewsStats, cfg.AdoWikiPageViewsForDays);
            var monthlyReport    = new MonthlyStatsReport(pastCommits, AuthorFilter, PathFilter);
            var wikiToc          = new WikiTableOfContents(
                fs.FileTree(cfg.GitRepoClonePath), // kj2 need to also exclude .git .attachments, etc.
                Task.FromResult((IEnumerable<WikiPageStats>) new List<WikiPageStats>()));

            var docsToWrite = new MarkdownDocument[]
            {
                // authorsReport,
                //filesReport,
                pagesViewsReport,
                //monthlyReport,
                //wikiToc
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
// 2. Instead of outputting to TextWriter, point to local wiki git clone.
// - Output to configurable file paths, to be .md files in the git clone.
/*
 * Work progress milestones:
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
