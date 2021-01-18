using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record PagesViewsStatsReport : MarkdownDocument
    {
        public const string DescriptionFormat = "Page views since last {0} days as of {1}. Total wiki pages: {2}";
        public static readonly object[] HeaderRow = { "Place", "Path", "Views" };

        public PagesViewsStatsReport(ITimeline timeline, Task<WikiPageStats[]> stats, int pageViewsForDays) :
            base(GetContent(timeline, pageViewsForDays, stats)) { }

        private static async Task<object[]> GetContent(
            ITimeline timeline,
            int pageViewsForDays,
            Task<WikiPageStats[]> stats)
        {
            var awaitedStats = await stats;
            return new object[]
            {
                string.Format(DescriptionFormat, pageViewsForDays, timeline.UtcNow, awaitedStats.Length),
                "",
                new TabularData(GetRows(awaitedStats))
            };
        }

        private static (object[] headerRow, object[][] rows) GetRows(WikiPageStats[] stats)
        {
            (string path, int views)[] pathsStats = stats
                .Select(pageStats =>
                    (
                        path: pageStats.Path,
                        views: pageStats.DayViewCounts.Sum()
                    )
                )
                .Where(stat => stat.views > 0)
                .OrderByDescending(stat => stat.views)
                .ToArray();

            var rows = pathsStats
                .Select((stats, i) => new object[] { $"{i + 1}", stats.path, stats.views })
                .ToArray();

            return (headerRow: HeaderRow, rows);
        }
    }
}