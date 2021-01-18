using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
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

            // Obtain inputs. Has out-of-process dependencies.
            GitLogCommit[]  recentCommits   = await gitLog.Commits(cfg.GitLogDays);
            GitLogCommit[]  pastCommits     = await gitLog.Commits(cfg.MonthlyReportStartDate, cfg.MonthlyReportEndDate);
            WikiPageStats[] pagesViewsStats = await wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

            var authorsReport    = new GitAuthorsStatsReport(timeline, cfg.GitLogDays, recentCommits, AuthorFilter);
            var filesReport      = new GitFilesStatsReport(timeline, cfg.GitLogDays, recentCommits, PathFilter);
            var pagesViewsReport = new PagesViewsStatsReport(timeline, cfg.AdoWikiPageViewsForDays, pagesViewsStats);
            var monthlyReport    = new MonthlyStatsReport(pastCommits, AuthorFilter, PathFilter);

            // Write outputs. Side-effectful.
            await authorsReport.WriteAsync(Console.Out);
            await filesReport.WriteAsync(Console.Out);
            await pagesViewsReport.WriteAsync(Console.Out);
            await monthlyReport.WriteAsync(Console.Out);
        }
    }
}

// kja make the digest check the day, and if it is time for a new one, do the following:
// - pull the new digest data from git and ado wiki api
// - save the new digest data to a json file
// - create a new md digest file in local repo clone
// - inform on stdout its time to manually review, commit and push the change