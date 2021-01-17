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

            var authorsReport    = new GitAuthorsStatsReport2(timeline, cfg.GitLogDays, authorsChangesStats);
            var filesReport      = new GitFilesStatsReport2(timeline, cfg.GitLogDays, filesChangesStats);
            var pagesViewsReport = new PagesViewsStatsReport2(timeline, cfg.AdoWikiPageViewsForDays, pagesViewsStats);
            var monthlyReport    = new MonthlyStatsReport(timeline, commits);

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

/*
            var st = Stopwatch.StartNew();
            Task<GitLogCommit[]>[] stats = Enumerable.Range(1, 12).Select(async i =>
            {
                var firstMonthDay  = new DateTime(2020, i, 1);
                var monthStart     = firstMonthDay.AddDays(-1);
                var nextMonthStart = firstMonthDay.AddMonths(1).AddDays(-1);

                var x = await gitLog.GetAuthorChangesStats2(since: monthStart, before: nextMonthStart);
                //var x = await gitLog.GetAuthorChangesStats2(since: new DateTime(2020, 1, 1).AddDays(-1), before: new DateTime(2021, 1, 1).AddDays(-1));
                Console.Out.WriteLine("Done " + i);
                return x;
            }).ToArray();
            Task.WaitAll(stats);
            Console.Out.WriteLine(st.Elapsed);
            st = Stopwatch.StartNew();
 */
/*
reportData = 
{
Description = "Wiki insertions by month, from MM-DD-YYYY to MM-DD-YYYY, both inclusive."
TabularData = new TabularData(
  HeaderRow = Month, Insertions, Monthly Change
  Rows = 
    List<List<GitFileChangeStats>> monthly change lists = foreach <month start and end dates>: GitLog.GetFilesChanges(start month, end month)
    monthly insertions sums = foreach monthly change list: sum insertions
    aggregate: monthly insertion sums: add MoM change. 
    transpose aggregate: instead of two lists (List(sums), List(MoM)), make it List(Month(sum), Month(MoM)) (see https://morelinq.github.io/3.0/ref/api/html/M_MoreLinq_MoreEnumerable_Transpose__1.htm)
)
}
*/