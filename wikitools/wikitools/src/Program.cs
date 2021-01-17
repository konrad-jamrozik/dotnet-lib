using System;
using System.Collections.Generic;
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

            var gitLog = GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath, cfg.GitLogDays);
            var wiki   = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar, cfg.AdoWikiPageViewsForDays);

            // Obtain inputs. Has out-of-process dependencies.
            List<GitAuthorChangeStats>       authorsChangesStats        = await gitLog.GetAuthorChangesStats(cfg.GitLogDays);
            List<GitFileChangeStats>         filesChangesStats          = await gitLog.GetFileChangesStats();
            List<WikiPageStats>              pagesViewsStats            = await wiki.GetPagesStats();
            List<List<GitAuthorChangeStats>> monthlyAuthorsChangesStats = GetMonthlyAuthorsChangesStats(gitLog);

            var authorsReport    = new GitAuthorsStatsReport2(timeline, cfg.GitLogDays, authorsChangesStats);
            var filesReport      = new GitFilesStatsReport2(timeline, cfg.GitLogDays, filesChangesStats);
            var pagesViewsReport = new PagesViewsStatsReport2(timeline, cfg.AdoWikiPageViewsForDays, pagesViewsStats);
            // kja curr work
            //var monthlyReport    = new MonthlyStatsReport2(timeline, monthlyAuthorsChangesStats);

            // Write outputs. Side-effectful.
            await authorsReport.WriteAsync(Console.Out);
            await filesReport.WriteAsync(Console.Out);
            await pagesViewsReport.WriteAsync(Console.Out);
            
            // await monthlyReport.WriteAsync(Console.Out);
        }

        private static List<List<GitAuthorChangeStats>> GetMonthlyAuthorsChangesStats(GitLog gitLog)
        {
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
            
            // kja this is super slow, likely better to call it once and process all the data
            var stats = Enumerable.Range(1, 12).Select(async i =>
            {
                var firstMonthDay  = new DateTime(2020, i, 1);
                var monthStart     = firstMonthDay.AddDays(-1);
                var nextMonthStart = firstMonthDay.AddMonths(1).AddDays(-1);

                var x = await gitLog.GetAuthorChangesStats(since: monthStart, before: nextMonthStart);
                Console.Out.WriteLine("Processing month "+ i);
                return x;
            }).Select(t => t.Result).ToList();
            return stats;
        }
    }
}