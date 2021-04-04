using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    public record AdoWikiWithStorage(
        IAdoWiki Wiki,
        AdoWikiPagesStatsStorage Storage,
        int? PageViewsForDaysWikiLimit = null) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays)
        {
            var updatedStorage  = Storage.Update(Wiki, pageViewsForDays.MinWith(PageViewsForDaysWikiLimit));
            var pagesViewsStats = updatedStorage.Select(s => s.PagesStats(pageViewsForDays));
            return pagesViewsStats;
        }
    }
}