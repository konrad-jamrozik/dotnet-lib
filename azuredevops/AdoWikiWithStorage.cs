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

    private async Task<ValidWikiPagesStats> PagesStats(int days, int? pageId)
    {
        var wikiDays = days.MinWith(AdoWiki.PageViewsForDaysMax());
        var updatedStorage = await UpdateStorageFromWiki(wikiDays, pageId);
        var updatedPagesViewsStats = updatedStorage
            .PagesStats(days.AsDaySpanUntil(AdoWiki.Today()))
            .WhereStats(StatsFilter(pageId));
        return updatedPagesViewsStats;
    }

    private async Task<AdoWikiPagesStatsStorage> UpdateStorageFromWiki(
        int days,
        int? pageId = null)
        => await Storage.Update(
            await (
                pageId == null
                    ? AdoWiki.PagesStats(days)
                    : AdoWiki.PageStats(days, (int)pageId)));

    private static Func<WikiPageStats, bool> StatsFilter(int? pageId)
        => pageId != null 
            ? page => page.Id == pageId
            : _ => true;
}