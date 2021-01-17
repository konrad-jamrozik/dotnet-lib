using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using static Wikitools.Declare;
using GitLog = Wikitools.Lib.Git.GitLog;

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

            // kja dehardcode the variables below
            var cfgStartYear = new DateTime(2019, 1, 1);
            var cfgEndYear   = new DateTime(2020, 1, 1);
            // kja use in all reports, dehardcode "/Meta". Same with "Konrad J" filter.
            Func<string, bool> filePathFilter = filePath => !filePath.Contains("/Meta");

            var gitLog = GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath, cfg.GitLogDays);
            var wiki   = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar, cfg.AdoWikiPageViewsForDays);

            // Obtain inputs. Has out-of-process dependencies.
            GitLogCommit[]      recentCommits   = await gitLog.Commits(cfg.GitLogDays);
            GitLogCommit[]      allCommits      = await gitLog.Commits(cfgStartYear, cfgEndYear);
            List<WikiPageStats> pagesViewsStats = await wiki.PagesStats();

            var authorsReport    = new GitAuthorsStatsReport2(timeline, cfg.GitLogDays, recentCommits);
            var filesReport      = new GitFilesStatsReport2(timeline, cfg.GitLogDays, recentCommits, filePathFilter);
            var pagesViewsReport = new PagesViewsStatsReport2(timeline, cfg.AdoWikiPageViewsForDays, pagesViewsStats);
            var monthlyReport    = new MonthlyStatsReport(timeline, allCommits, filePathFilter);

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