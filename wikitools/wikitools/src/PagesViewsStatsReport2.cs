using System.Collections.Generic;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record PagesViewsStatsReport2(ITimeline Timeline, int Days, WikiPageStats[] Stats) : MarkdownDocument
    {
        public override List<object> Content =>
            new()
            {
                $"Page views since last {Days} days as of {Timeline.UtcNow}. Total wiki pages: {Stats.Length}",
                "",
                new TabularData2(GetRows(Stats))
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