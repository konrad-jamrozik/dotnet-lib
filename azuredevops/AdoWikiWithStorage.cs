using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public record AdoWikiWithStorage(
    IAdoWiki AdoWiki,
    AdoWikiPagesStatsStorage Storage,
    int? PageViewsForDaysMax = null) : IAdoWiki
{
    private const int DefaultPageViewsForDaysMax = PageViewsForDays.Max;

    public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays)
    {
        // kja Contract.Assert(pageViewsForDays >= 1); / instead strongly type the input
        Contract.Assert(pageViewsForDays >= 1);
        var updatedStorage = Storage.Update(
            AdoWiki,
            pageViewsForDays.MinWith(
                PageViewsForDaysMax ?? DefaultPageViewsForDaysMax));
        var pagesViewsStats = updatedStorage.Select(s => s.PagesStats(pageViewsForDays));
        return pagesViewsStats;
    }

    public Task<ValidWikiPagesStats> PageStats(int pageViewsForDays, int pageId)
    {
        var dayRange = pageViewsForDays.MinWith(PageViewsForDaysMax);
        var updatedStorage  = Storage.Update(AdoWiki, dayRange, pageId);
        var pagesViewsStats = updatedStorage.Select(
            storage =>
            {
                var endDay = new DateDay(Storage.CurrentDate);
                var startDay = endDay.AddDays(-dayRange + 1);
                return new ValidWikiPagesStats(
                    storage.PagesStats(dayRange).Where(page => page.Id == pageId),
                    startDay, 
                    endDay);
            });
        return pagesViewsStats;
    }
}