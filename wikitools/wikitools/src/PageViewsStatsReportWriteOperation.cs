using System.IO;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    class PageViewsStatsReportWriteOperation
    {
        private readonly MarkdownTable _table;

        public PageViewsStatsReportWriteOperation(
            string wikiUri,
            string patEnvVar, 
            int pageViewsForDays)
        {
            var wiki = new AdoWiki(new AdoWikiUri(wikiUri), patEnvVar, pageViewsForDays);
            var report = new PageViewsStatsReport(wiki, pageViewsForDays);
            _table = new MarkdownTable(report);
        }

        public Task ExecuteAsync(TextWriter writer) 
            => _table.WriteAsync(writer);
    }
}