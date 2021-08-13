using System.Linq;
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

        public Task<ValidWikiPagesStats> PageStats(int pageViewsForDays, int pageId)
        {
            var dayRange = pageViewsForDays.MinWith(PageViewsForDaysWikiLimit);
            var updatedStorage  = Storage.Update(AdoWiki, dayRange, pageId);
            var pagesViewsStats = updatedStorage.Select(
                s =>
                {
                    var endDay = new DateDay(Storage.CurrentDate);
                    var startDay = endDay.AddDays(-dayRange + 1);
                    return new ValidWikiPagesStats(
                        s.PagesStats(dayRange).Where(page => page.Id == pageId),
                        startDay, 
                        endDay);
                });
            return pagesViewsStats;
        }
    }
}