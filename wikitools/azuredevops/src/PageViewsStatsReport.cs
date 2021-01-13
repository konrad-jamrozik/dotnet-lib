using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools.AzureDevOps
{
    public record PageViewsStatsReport : ITabularData
    {
        public const string DescriptionFormat = "Page views since last {0} days as of {1}. Total wiki pages: {2}";
        public static readonly List<object> HeaderRowLabels = new() { "Place", "Path", "Views" };

        private readonly AsyncLazy<List<List<object>>> _rows;
        private readonly ITimeline _timeline;
        private readonly int _days;
        private readonly AsyncLazy<List<WikiPageStats>> _pagesStats;

        public PageViewsStatsReport(ITimeline timeline, AdoWiki adoWiki, int days)
        {
            _timeline = timeline;
            _days = days;
            _pagesStats = new AsyncLazy<List<WikiPageStats>>(async () => await adoWiki.GetPagesStats());
            _rows = new AsyncLazy<List<List<object>>>(Rows);

            async Task<List<List<object>>> Rows()
            {
                List<WikiPageStats> pagesStats = await _pagesStats.Value;

                List<(string path, int views)> pathsStats = pagesStats
                    .Select(pageStats =>
                        (
                            path: pageStats.Path,
                            views: pageStats.DayViewCounts.Sum()
                        )
                    )
                    .Where(stat => stat.views > 0)
                    .OrderByDescending(stat => stat.views)
                    .ToList();

                List<List<object>> rows = Enumerable.Range(0, pathsStats.Count)
                    .Select(i => new List<object> { $"{i + 1}", pathsStats[i].path, pathsStats[i].views })
                    .ToList();

                return rows;
            }
        }

        public async Task<string> GetDescription() => string.Format(DescriptionFormat,
            _days,
            _timeline.UtcNow,
            (await _pagesStats.Value).Count);

        public List<object> HeaderRow => HeaderRowLabels;
        public Task<List<List<object>>> GetRows() => _rows.Value;
    }
}