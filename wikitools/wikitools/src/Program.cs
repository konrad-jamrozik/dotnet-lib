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
            var cfg = WikitoolsConfig.From("wikitools_config.json");

            var timeline = new Timeline();
            var os       = new WindowsOS();
            var adoApi   = new AdoApi();

            var outputSink = Console.Out;

            await Execute(cfg, timeline, os, adoApi, outputSink);
        }

        public static Task Execute(
            WikitoolsConfig cfg,
            ITimeline timeline,
            IOperatingSystem os,
            IAdoApi adoApi,
            TextWriter outputSink)
        {
            var gitLog = GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath);
            var wiki   = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar);

            var recentCommits   = gitLog.Commits(cfg.GitLogDays);
            var pastCommits     = gitLog.Commits(cfg.MonthlyReportStartDate, cfg.MonthlyReportEndDate);
            
            // Previous code, before storage was added. To remove when migration is done.
            //var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);
            var storage = new WikiPagesStatsStorage(os, cfg.StorageDirPath, wiki); // kja wiki passed here temporarily
            var updatedStorage = storage.Update(wiki);
            var pagesViewsStats = updatedStorage.M(s => s.PagesStats(cfg.AdoWikiPageViewsForDays));

            bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
            bool PathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);

            var authorsReport =
                new GitAuthorsStatsReport(timeline, recentCommits, cfg.GitLogDays, cfg.Top, AuthorFilter);
            var filesReport = new GitFilesStatsReport(timeline, recentCommits, cfg.GitLogDays, cfg.Top, PathFilter);
            var pagesViewsReport = new PagesViewsStatsReport(timeline, pagesViewsStats, cfg.AdoWikiPageViewsForDays);
            var monthlyReport = new MonthlyStatsReport(pastCommits, AuthorFilter, PathFilter);

            var docsToWrite = new MarkdownDocument[]
            {
                authorsReport,
                filesReport,
                pagesViewsReport,
                monthlyReport
            };

            return WriteAll(docsToWrite, outputSink);
        }

        private static Task WriteAll(MarkdownDocument[] docs, TextWriter textWriter) =>
            Task.WhenAll(docs.Select(doc => doc.WriteAsync(textWriter)).ToArray());
    }
}

// kja checkpoint 1/18/2021. Next task: collect and persist data from ADO wiki.
// - persist data pulled from ADO as a json file on the file system
// - one json file per each month
// - on new program execution, check missing whole days. If present, get them from ADO and add to the saved data
// - compute the data from the saved json, not from data coming directly from ADO