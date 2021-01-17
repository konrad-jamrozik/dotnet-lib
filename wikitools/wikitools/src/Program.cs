using System;
using System.Collections.Generic;
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

            var gitLog = GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath, cfg.GitLogDays);
            var wiki   = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar, cfg.AdoWikiPageViewsForDays);

            // Obtain inputs. Has out-of-process dependencies.
            List<GitAuthorChangeStats> authorsChangesStats = await gitLog.GetAuthorChangesStats(cfg.GitLogDays);
            List<GitFileChangeStats>   filesChangesStats   = await gitLog.GetFileChangesStats();
            List<WikiPageStats>        pagesViewsStats     = await wiki.GetPagesStats();

            GitLogCommit[] recentCommits = await GetCommits(gitLog, cfg.GitLogDays);

            // kja dehardcode dates
            GitLogCommit[] allCommits = await GetCommits(gitLog, new DateTime(2019, 1, 1), new DateTime(2020, 1, 1));

            // kja use in all reports, dehardcode "/Meta". Same with "Konrad J" filter.
            Func<string, bool> filePathFilter = filePath => !filePath.Contains("/Meta");
            var authorsReport = new GitAuthorsStatsReport2(timeline, cfg.GitLogDays, recentCommits);
            var filesReport = new GitFilesStatsReport2(timeline, cfg.GitLogDays, recentCommits, filePathFilter);
            var pagesViewsReport = new PagesViewsStatsReport2(timeline, cfg.AdoWikiPageViewsForDays, pagesViewsStats);
            var monthlyReport = new MonthlyStatsReport(timeline, allCommits, filePathFilter);

            // Write outputs. Side-effectful.
            // kja temp off
            await authorsReport.WriteAsync(Console.Out);
            await filesReport.WriteAsync(Console.Out);
            // await pagesViewsReport.WriteAsync(Console.Out);
            // await monthlyReport.WriteAsync(Console.Out);
        }

        // kja this should be GitLog method
        private static Task<GitLogCommit[]> GetCommits(GitLog gitLog, int days) =>
            gitLog.GetAuthorChangesStats2(days);

        // kja this should be GitLog method
        private static Task<GitLogCommit[]> GetCommits(GitLog gitLog, DateTime startYear, DateTime endYear) =>
            gitLog.GetAuthorChangesStats2(
                // .AddDays(-1) necessary as for "git log" the --since date day is exclusive
                since: startYear.AddDays(-1),
                // .AddYears(1).AddDays(-1), to include the last day of endYear.
                // In "git log" --before date day is inclusive.
                before: endYear.AddYears(1).AddDays(-1)
            );
    }
}

// kja make the digest check the day, and if it is time for a new one, do the following:
// - pull the new digest data from git and ado wiki api
// - save the new digest data to a json file
// - create a new md digest file in local repo clone
// - inform on stdout its time to manually review, commit and push the change