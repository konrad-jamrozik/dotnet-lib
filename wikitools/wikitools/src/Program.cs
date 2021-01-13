using System;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public static class Program
    {
        public static async Task Main()
        {
            var cfg = WikitoolsConfig.From("wikitools_config.json");

            var timeline = new Timeline();
            var os       = new WindowsOS();
            var adoApi   = new AdoApi();

            var gitLog = Declare.GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath, cfg.GitLogDays);
            var wiki   = Declare.Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar, cfg.AdoWikiPageViewsForDays);

            var gitAuthorsReport = new GitAuthorsStatsReport(timeline, gitLog, cfg.GitLogDays);
            var gitFilesReport   = new GitFilesStatsReport(timeline, gitLog, cfg.GitLogDays);
            var pageViewsReport  = new PageViewsStatsReport(timeline, wiki, cfg.AdoWikiPageViewsForDays);

            await new MarkdownTable(gitAuthorsReport).WriteAsync(Console.Out);
            await new MarkdownTable(gitFilesReport).WriteAsync(Console.Out);
            await new MarkdownTable(pageViewsReport).WriteAsync(Console.Out);
        }
    }
}