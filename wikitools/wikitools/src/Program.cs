using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using static Wikitools.Declare;

namespace Wikitools
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            ITimeline        timeline = new Timeline();
            IOperatingSystem os       = new WindowsOS();
            IAdoApi          adoApi   = new AdoApi(os.Environment);

            var cfg = WikitoolsConfig.From(os.FileSystem, "wikitools_config.json");

            var docsToWrite = DocsToWrite(timeline, os, adoApi, cfg);
            var outputSink  = Console.Out;

            await WriteAll(docsToWrite, outputSink);
        }

        private static MarkdownDocument[] DocsToWrite(
            ITimeline timeline,
            IOperatingSystem os,
            IAdoApi adoApi,
            WikitoolsConfig cfg)
        {
            var gitLog = GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath);
            var wiki = WikiWithStorage(
                adoApi,
                cfg.AdoWikiUri,
                cfg.AdoPatEnvVar,
                os.FileSystem,
                cfg.StorageDirPath,
                timeline.UtcNow);

            var recentCommits = gitLog.Commits(cfg.GitLogDays);
            var pastCommits   = gitLog.Commits(cfg.MonthlyReportStartDate, cfg.MonthlyReportEndDate);

            var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
            bool PathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);

            var authorsReport    = new GitAuthorsStatsReport(timeline, recentCommits, cfg.GitLogDays, cfg.Top, AuthorFilter);
            var filesReport      = new GitFilesStatsReport(timeline, recentCommits, cfg.GitLogDays, cfg.Top, PathFilter);
            var pagesViewsReport = new PagesViewsStatsReport(timeline, pagesViewsStats, cfg.AdoWikiPageViewsForDays);
            var monthlyReport    = new MonthlyStatsReport(pastCommits, AuthorFilter, PathFilter);
            var wikiToc          = new WikiTableOfContents(Task.FromResult(new object[0]));

            var docsToWrite = new MarkdownDocument[]
            {
                authorsReport,
                filesReport,
                pagesViewsReport,
                monthlyReport,
                wikiToc
            };
            return docsToWrite;
        }

        private static Task WriteAll(MarkdownDocument[] docs, TextWriter textWriter) =>
            Task.WhenAll(docs.Select(doc => doc.WriteAsync(textWriter)).ToArray());
    }
}

// kja high-level todos
// 1. One page TOC
// - Annotations (icons): Newly added, lots of traffic, stale.
// 2. Instead of outputting to TextWriter, point to local wiki git clone.
// - Output to configurable file paths, to be .md files in the git clone.
/*
 * Work progress milestones:
 * 1/18/2021:
 * - Markdown reports with data pulled from git.
 * - Markdown report with page view stats data pulled from ADO API.
 * - Explore from: Program, GitAuthorsStatsReport, GitFilesStatsReport, MonthlyStatsReport, PagesViewsStatsReport
 * 2/27/2021:
 * - Persisting wiki data to HDD as monthly json, allowing to report more than 30 days.
 * - Tooling for merging and splitting existing wiki data.
 * - Explore from: Program, AdoWikiWithStorage
 */
