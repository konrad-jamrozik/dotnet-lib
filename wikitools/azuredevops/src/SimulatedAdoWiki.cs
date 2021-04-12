using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoWiki(IEnumerable<WikiPageStats> PagesStatsData) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays) =>
            Task.FromResult(new ValidWikiPagesStats(PagesStatsData));

        public Task<ValidWikiPagesStats> PageStats(int pageViewsForDays, int pageId) => Task.FromResult(
            new ValidWikiPagesStats(PagesStatsData.Where(page => page.Id == pageId)));
    }
}