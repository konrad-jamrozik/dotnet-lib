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
            var cfg        = WikitoolsConfig.From("wikitools_config.json");
            var timeline   = new Timeline();
            var os         = new WindowsOS();
            var adoApi     = new AdoApi();
            var outputSink = Console.Out;

            await Main(cfg, timeline, os, adoApi, outputSink);
        }

        public static async Task Main(
            WikitoolsConfig cfg,
            Timeline timeline,
            WindowsOS os,
            AdoApi adoApi,
            TextWriter outputSink)
        {
            var gitLog = GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath);
            var wiki   = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar);

            var recentCommits   = gitLog.Commits(cfg.GitLogDays);
            var pastCommits     = gitLog.Commits(cfg.MonthlyReportStartDate, cfg.MonthlyReportEndDate);
            var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
            bool PathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);

            var authorsReport =
                new GitAuthorsStatsReport(timeline, recentCommits, cfg.GitLogDays, cfg.Top, AuthorFilter);
            var filesReport      = new GitFilesStatsReport(timeline, recentCommits, cfg.GitLogDays, cfg.Top, PathFilter);
            var pagesViewsReport = new PagesViewsStatsReport(timeline, pagesViewsStats, cfg.AdoWikiPageViewsForDays);
            var monthlyReport    = new MonthlyStatsReport(pastCommits, AuthorFilter, PathFilter);

            var docsToWrite = new MarkdownDocument[]
            {
                authorsReport,
                filesReport,
                pagesViewsReport,
                monthlyReport
            };

            await WriteAll(docsToWrite, outputSink);
        }

        private static async Task WriteAll(MarkdownDocument[] docs, TextWriter textWriter) =>
            await Task.WhenAll(docs.Select(doc => doc.WriteAsync(textWriter)).ToArray());
    }
}

// kja checkpoint 1/18/2021. Next task: collect and persist data from ADO wiki.
// - persist data pulled from ADO as a json file on the file system
// - one json file per each month
// - on new program execution, check missing whole days. If present, get them from ADO and add to the saved data
// - compute the data from the saved json, not from data coming directly from ADO