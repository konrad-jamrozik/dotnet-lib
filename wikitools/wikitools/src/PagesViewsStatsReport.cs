using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Data;

namespace Wikitools
{
    public record PagesViewsStatsReport : MarkdownDocument
    {
        public const string DescriptionFormat = "Page views since last {0} days as of {1}. Total wiki pages: {2}";
        public static readonly object[] HeaderRow = { "Place", "Path", "Views" };

        public PagesViewsStatsReport(ITimeline timeline, Task<ValidWikiPagesStats> stats, int pageViewsForDays) :
            base(GetContent(timeline, pageViewsForDays, stats)) { }

        private static async Task<object[]> GetContent(
            ITimeline timeline,
            int pageViewsForDays,
            Task<ValidWikiPagesStats> stats)
        {
            var awaitedStats = await stats;
            return new object[]
            {
                string.Format(DescriptionFormat, pageViewsForDays, timeline.UtcNow, awaitedStats.Count()),
                "",
                new TabularData(GetRows(awaitedStats))
            };
        }

        private static (object[] headerRow, object[][] rows) GetRows(ValidWikiPagesStats stats)
        {
            (string path, int views)[] pathsStats = stats 
                .Select(pageStats =>
                    (
                        path: pageStats.Path,
                        views: pageStats.DayStats.Sum(s => s.Count)
                    )
                )
                .Where(stat => stat.views > 0)
                .OrderByDescending(stat => stat.views)
                .ToArray();

            var rows = pathsStats
                .Select((path, views) => new object[] { $"{views + 1}", path.path, path.views })
                .ToArray();

            return (headerRow: HeaderRow, rows);
        }
    }
}