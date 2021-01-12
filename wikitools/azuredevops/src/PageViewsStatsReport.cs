using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
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
        private readonly AsyncLazy<List<WikiPageDetail>> _pagesDetails;

        public PageViewsStatsReport(ITimeline timeline, AdoWiki adoWiki, int days)
        {
            _rows = new AsyncLazy<List<List<object>>>(Rows);
            _timeline = timeline;
            _days = days;
            _pagesDetails = new AsyncLazy<List<WikiPageDetail>>(
                async () => await adoWiki.GetPagesDetails());

            async Task<List<List<object>>> Rows()
            {
                List<WikiPageDetail> pagesDetails = await _pagesDetails.Value;

                List<(string path, int views)> pathsStats = pagesDetails
                    .Select(wikiPageDetail => 
                        (
                            path: wikiPageDetail.Path, 
                            views: wikiPageDetail.ViewStats?.Sum(stat => stat.Count) ?? 0
                        )
                    )
                    .Where(stat => stat.views > 0)
                    .OrderByDescending(stat => stat.views)
                    .ToList();

                List<List<object>> rows = Enumerable.Range(0, pathsStats.Count)
                    .Select(i => new List<object> {$"{i + 1}", pathsStats[i].path, pathsStats[i].views})
                    .ToList();

                return rows;
            }
        }

        public async Task<string> GetDescription() => string.Format(DescriptionFormat,
            _days,
            _timeline.UtcNow,
            (await _pagesDetails.Value).Count);
        public List<object> HeaderRow => HeaderRowLabels;
        public Task<List<List<object>>> GetRows() => _rows.Value;
    }
}