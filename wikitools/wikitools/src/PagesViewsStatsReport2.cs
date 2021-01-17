using System.Collections.Generic;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public class PagesViewsStatsReport2 : MarkdownDocument
    {
        private readonly ITimeline _timeline;
        private readonly int _days;
        private readonly List<WikiPageStats> _stats;

        public PagesViewsStatsReport2(ITimeline timeline, int days, List<WikiPageStats> stats)
        {
            _timeline = timeline;
            _days = days;
            _stats = stats;
        }

        public override List<object> Content =>
            new()
            {
                $"Page views since last {_days} days as of {_timeline.UtcNow}. Total wiki pages: {_stats.Count}",
                "",
                new TabularData2(GetRows(_stats))
            };

        private static (string[] headerRow, object[][] rows) GetRows(List<WikiPageStats> stats)
        {
            (string path, int views)[] pathsStats = stats
                .Select(pageStats =>
                    (
                        path:  pageStats.Path,
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