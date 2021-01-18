using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
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
            var cfg      = WikitoolsConfig.From("wikitools_config.json");
            var timeline = new Timeline();
            var os       = new WindowsOS();
            var adoApi   = new AdoApi();

            bool AuthorFilter(string author) => !cfg.ExcludedAuthors.Any(author.Contains);
            bool PathFilter(string path) => !cfg.ExcludedPaths.Any(path.Contains);

            var gitLog = GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath);
            var wiki   = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar);

            var recentCommits   = gitLog.Commits(cfg.GitLogDays);
            var pastCommits     = gitLog.Commits(cfg.MonthlyReportStartDate, cfg.MonthlyReportEndDate);
            var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            var authorsReport =
                new GitAuthorsStatsReport(timeline, cfg.GitLogDays, recentCommits, cfg.Top, AuthorFilter);
            var filesReport = new GitFilesStatsReport(timeline, cfg.GitLogDays, recentCommits, cfg.Top, PathFilter);
            var pagesViewsReport = new PagesViewsStatsReport(timeline, cfg.AdoWikiPageViewsForDays, pagesViewsStats);
            var monthlyReport = new MonthlyStatsReport(pastCommits, AuthorFilter, PathFilter);

            // Output sink
            var textWriter = Console.Out;
            
            var tasks = new MarkdownDocument[]
            {
                authorsReport, 
                filesReport, 
                pagesViewsReport, 
                monthlyReport
            };

            // Write outputs. Side-effectful.
            await WriteAll(tasks, textWriter);
        }

        private static async Task WriteAll(MarkdownDocument[] markdownDocuments, TextWriter textWriter) =>
            await Task.WhenAll(markdownDocuments.Select(report => report.WriteAsync(textWriter)).ToArray());
    }
}

// kja make the digest check the day, and if it is time for a new one, do the following:
// - pull the new digest data from git and ado wiki api
// - save the new digest data to a json file
// - create a new md digest file in local repo clone
// - inform on stdout its time to manually review, commit and push the change