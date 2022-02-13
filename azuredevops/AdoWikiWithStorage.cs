using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public record AdoWikiWithStorage(
    IAdoWiki AdoWiki,
    AdoWikiPagesStatsStorage Storage,
    int? PageViewsForDaysMax = null) : IAdoWiki
{
    private const int DefaultPageViewsForDaysMax = PageViewsForDays.Max;
    public Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pvfd)
    {
        var updatedStorage = Storage.Update(
            AdoWiki,
            pvfd.Value.MinWith(PageViewsForDaysMax ?? DefaultPageViewsForDaysMax));
        var pagesViewsStats = updatedStorage.Select(s => s.PagesStats(pvfd));
        return pagesViewsStats;
    }

    public Task<ValidWikiPagesStats> PageStats(PageViewsForDays pvfd, int pageId)
    {
        // kj2 test for ?? DefaultPageViewsForDaysMax
        // This test does it for PagesStats:
        // Wikitools.AzureDevOps.Tests.AdoWikiWithStorageTests.DataFromStorageFromManyMonths
        pvfd = pvfd.MinWith(PageViewsForDaysMax ?? DefaultPageViewsForDaysMax);
        var updatedStorage = Storage.Update(AdoWiki, pvfd, pageId);
        var pageViewsStats =
            updatedStorage.Select(
                s => s.PagesStats(pvfd).WherePages(page => page.Id == pageId));
        return pageViewsStats;
    }
}