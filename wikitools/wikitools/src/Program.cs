using System;
using System.Threading.Tasks;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public static class Program
    {
        public static async Task Main()
        {
            var cfg = WikitoolsConfig.From("wikitools_config.json");

            var timeline = new Timeline();
            var os = new WindowsOS();

            var authorsStatsReportWriteOperation = new GitAuthorsStatsReportWriteOperation(
                timeline,
                os,
                cfg.GitRepoClonePath,
                cfg.GitExecutablePath,
                cfg.GitLogDays);

            var pageViewsStatsReportWriteOperation = new PageViewsStatsReportWriteOperation(
                timeline,
                cfg.AdoWikiUri,
                cfg.AdoPatEnvVar, 
                cfg.AdoWikiPageViewsForDays);

            await authorsStatsReportWriteOperation.ExecuteAsync(Console.Out);
            await pageViewsStatsReportWriteOperation.ExecuteAsync(Console.Out);
        }
    }
}