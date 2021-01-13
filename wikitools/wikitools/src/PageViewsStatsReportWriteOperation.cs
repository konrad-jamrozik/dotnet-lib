using System.IO;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public class PageViewsStatsReportWriteOperation
    {
        private readonly MarkdownTable _table;

        public PageViewsStatsReportWriteOperation(
            ITimeline timeline,
            IAdoApi adoApi,
            string wikiUri,
            string patEnvVar,
            int pageViewsForDays)
        {
            var wiki   = new AdoWiki(adoApi, new AdoWikiUri(wikiUri), patEnvVar, pageViewsForDays);
            var report = new PageViewsStatsReport(timeline, wiki, pageViewsForDays);
            _table = new MarkdownTable(report);
        }

        public Task ExecuteAsync(TextWriter writer) 
            => _table.WriteAsync(writer);
    }
}