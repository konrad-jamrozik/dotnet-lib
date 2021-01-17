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
            GitLogCommit[]             commits             = await GetCommits(gitLog, 2019, 2020);

            // kja use in all reports, dehardcode "/Meta"
            Func<string, bool> filePathFilter = filePath => !filePath.Contains("/Meta");
            var authorsReport    = new GitAuthorsStatsReport2(timeline, cfg.GitLogDays, authorsChangesStats);
            var filesReport      = new GitFilesStatsReport2(timeline, cfg.GitLogDays, filesChangesStats);
            var pagesViewsReport = new PagesViewsStatsReport2(timeline, cfg.AdoWikiPageViewsForDays, pagesViewsStats);
            var monthlyReport    = new MonthlyStatsReport(timeline, commits, filePathFilter);

            // Write outputs. Side-effectful.
            // kja temp off
            // await authorsReport.WriteAsync(Console.Out);
            // await filesReport.WriteAsync(Console.Out);
            // await pagesViewsReport.WriteAsync(Console.Out);
            await monthlyReport.WriteAsync(Console.Out);
        }

        private static Task<GitLogCommit[]> GetCommits(GitLog gitLog, int startYear, int endYear) =>
            gitLog.GetAuthorChangesStats2(
                // AddDays(-1) necessary as for "git log" the --since date day is exclusive
                since: new DateTime(startYear, 1, 1).AddDays(-1), 
                // endYear + 1 and then AddDays(-1), to include the last day of endYear.
                // In "git log" --before date day is inclusive.
                before: new DateTime(endYear + 1, 1, 1).AddDays(-1));
    }
}