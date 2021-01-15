using System;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using static Wikitools.Declare;

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

            var gitLog = GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath, cfg.GitLogDays);
            var wiki   = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar, cfg.AdoWikiPageViewsForDays);

            var gitAuthorsReport = new GitAuthorsStatsReport(timeline, gitLog, cfg.GitLogDays);
            var gitFilesReport   = new GitFilesStatsReport(timeline, gitLog, cfg.GitLogDays);
            var pageViewsReport  = new PagesViewsStatsReport(timeline, wiki, cfg.AdoWikiPageViewsForDays);
            var wikiDigest       = new WikiDigest(gitAuthorsReport, gitFilesReport, pageViewsReport);

            await wikiDigest.WriteAsync(Console.Out);
        }
    }
}