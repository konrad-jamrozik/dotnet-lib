using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoWiki(
        IEnumerable<WikiPageStats> PagesStatsData,
        DateDay StatsRangeStartDay,
        DateDay StatsRangeEndDay) : IAdoWiki
    {
        public SimulatedAdoWiki(ValidWikiPagesStats stats) : this(
            stats, 
            stats.StartDay,
            stats.EndDay)
        {
        }

        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays) =>
            Task.FromResult(new ValidWikiPagesStats(PagesStatsData, StatsRangeStartDay, StatsRangeEndDay));

        public Task<ValidWikiPagesStats> PageStats(int pageViewsForDays, int pageId) => Task.FromResult(
            new ValidWikiPagesStats(PagesStatsData.Where(page => page.Id == pageId), StatsRangeStartDay, StatsRangeEndDay));
    }
}