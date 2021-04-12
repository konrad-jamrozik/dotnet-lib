using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    public record AdoWikiWithStorage(
        IAdoWiki AdoWiki,
        AdoWikiPagesStatsStorage Storage,
        int? PageViewsForDaysWikiLimit = null) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays)
        {
            var updatedStorage  = Storage.Update(AdoWiki, pageViewsForDays.MinWith(PageViewsForDaysWikiLimit));
            var pagesViewsStats = updatedStorage.Select(s => s.PagesStats(pageViewsForDays));
            return pagesViewsStats;
        }

        // kja dedup
        public Task<ValidWikiPagesStats> PageStats(int pageViewsForDays, int pageId)
        {
            var updatedStorage  = Storage.Update(AdoWiki, pageViewsForDays.MinWith(PageViewsForDaysWikiLimit), pageId);
            var pagesViewsStats = updatedStorage.Select(s => s.PagesStats(pageViewsForDays)); // kja pageStats
            return pagesViewsStats;
        }
    }
}