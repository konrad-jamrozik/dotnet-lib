using System;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public record AdoWikiWithStorage(
    IAdoWiki AdoWiki,
    AdoWikiPagesStatsStorage Storage) : IAdoWiki
{
    public Task<ValidWikiPagesStats> PagesStats(int days)
        => PagesStats(days, pageId: null);

    int IAdoWiki.PageViewsForDaysMax() => AdoWiki.PageViewsForDaysMax();

    public Task<ValidWikiPagesStats> PageStats(int days, int pageId)
        => PagesStats(days, pageId);

    public DateDay Today() => AdoWiki.Today();

    private Task<ValidWikiPagesStats> PagesStats(int days, int? pageId)
    {
        var updatedStorage = UpdateFromWiki(AdoWiki, days, pageId);
        var updatedPagesViewsStats =
            updatedStorage.Select(storage => storage
                .PagesStats(days.AsDaySpanUntil(AdoWiki.Today()))
                .WhereStats(StatsFilter(pageId)));
        return updatedPagesViewsStats;
    }

    private static Func<WikiPageStats, bool> StatsFilter(int? pageId)
        => pageId != null 
            ? page => page.Id == pageId
            : _ => true;

    private async Task<AdoWikiPagesStatsStorage> UpdateFromWiki(
        IAdoWiki wiki,
        int days,
        int? pageId = null)
    {
        var pvfd = new PageViewsForDays(days.MinWith(AdoWiki.PageViewsForDaysMax()));
        var wikiPagesStats = await (
            pageId == null
                ? wiki.PagesStats(pvfd)
                : wiki.PageStats(pvfd, (int)pageId));
        return await Storage.Update(wikiPagesStats);
    }
}