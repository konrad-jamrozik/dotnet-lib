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
        private readonly AsyncLazy<List<List<object>>> _rows;
        private readonly int _days;
        private readonly AsyncLazy<List<WikiPageDetail>> _pagesDetails;

        public PageViewsStatsReport(AdoWiki adoWiki, int days)
        {
            _rows = new AsyncLazy<List<List<object>>>(Rows);
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

        public async Task<string> GetDescription() => 
            $"Page views since last {_days} days as of {DateTime.UtcNow}. " +
            $"Total wiki pages: {(await _pagesDetails.Value).Count}";
        public List<object> HeaderRow => new() { "Place", "Path", "Views" };
        public Task<List<List<object>>> GetRows() => _rows.Value;
    }
}