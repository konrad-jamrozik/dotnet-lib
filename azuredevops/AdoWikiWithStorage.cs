using System;
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
        => PagesStats(pvfd, pageId: null);

    public Task<ValidWikiPagesStats> PageStats(PageViewsForDays pvfd, int pageId)
        => PagesStats(pvfd, pageId);

    public DateDay Today() => AdoWiki.Today();

    private Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pvfd, int? pageId)
    {
        Func<WikiPageStats, bool> statsFilter =
            pageId != null 
                ? page => page.Id == pageId 
                : _ => true;

        var pvfdForApi = pvfd.MinWith(PageViewsForDaysMax ?? DefaultPageViewsForDaysMax);
        var updatedStorage = Storage.Update(AdoWiki, pvfdForApi, pageId);
        // kja check if all the other pvfds here can be bound
        var boundPvfd = new BoundPageViewsForDays(pvfd, AdoWiki.Today());
        var pagesViewsStats = updatedStorage.Select(s => s.PagesStats(boundPvfd).WhereStats(statsFilter));
        return pagesViewsStats;
    }
}