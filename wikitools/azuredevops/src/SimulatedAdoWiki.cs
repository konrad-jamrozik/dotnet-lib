using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoWiki(IEnumerable<WikiPageStats> PagesStatsData) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays) =>
            Task.FromResult(new ValidWikiPagesStats(PagesStatsData));
    }
}