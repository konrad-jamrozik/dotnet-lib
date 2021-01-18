using System.Collections.Generic;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record PagesViewsStatsReport2 : MarkdownDocument
    {
        public PagesViewsStatsReport2(ITimeline timeline, int days, WikiPageStats[] stats) :
            base(GetContent(timeline, days, stats)) { }

        private static List<object> GetContent(
            ITimeline timeline,
            int days,
            WikiPageStats[] stats) =>
            new()
            {
                $"Page views since last {days} days as of {timeline.UtcNow}. Total wiki pages: {stats.Length}",
                "",
                new TabularData2(GetRows(stats))
            };

        private static (string[] headerRow, object[][] rows) GetRows(WikiPageStats[] stats)
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

            return (headerRow: new[] { "Place", "Path", "Views" }, rows);
        }
    }
}