using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    public record AdoWikiWithStorage(
        IAdoWiki AdoWiki,
        AdoWikiPagesStatsStorage Storage,
        int? PageViewsForDaysMax = null) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays)
        {
            // kj2 instead strongly type the input
            Contract.Assert(pageViewsForDays >= 1);
            // kja 5 if I try to get more than 30 days, this will pass to ADO, blowing up. Instead...
            // up to 30 should come from ADO, and the rest from storage. 
            // Write a test for this --> Ultimately the problem is that Wikitools.AzureDevOps.SimulatedAdoWiki doesn't
            // properly simulate the limit of AdoApi.
            var updatedStorage  = Storage.Update(AdoWiki, pageViewsForDays.MinWith(PageViewsForDaysMax));
            var pagesViewsStats = updatedStorage.Select(s => s.PagesStats(pageViewsForDays));
            return pagesViewsStats;
        }

        public Task<ValidWikiPagesStats> PageStats(int pageViewsForDays, int pageId)
        {
            var dayRange = pageViewsForDays.MinWith(PageViewsForDaysMax);
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