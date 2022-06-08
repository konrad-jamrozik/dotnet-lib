using System;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public record AdoWikiWithStorage(
    IAdoWiki AdoWiki,
    AdoWikiPagesStatsStorage Storage,
    // kja PageViewsForDaysMax should instead be derived from AdoWiki, and in Storage.Update, which should take "days" as input, not "pvfd".
    int PageViewsForDaysMax) : IAdoWiki
{
    public Task<ValidWikiPagesStats> PagesStats(int days)
        => PagesStats(days, pageId: null);

    public Task<ValidWikiPagesStats> PageStats(int days, int pageId)
        => PagesStats(days, pageId);

    public DateDay Today() => AdoWiki.Today();

    private Task<ValidWikiPagesStats> PagesStats(int days, int? pageId)
    {
        Func<WikiPageStats, bool> statsFilter =
            pageId != null 
                ? page => page.Id == pageId
                : _ => true;

        var pvfd = new PageViewsForDays(days.MinWith(PageViewsForDaysMax));
        var updatedStorage = Storage.Update(AdoWiki, pvfd, pageId);
        var storageDaySpan = days.AsDaySpanUntil(AdoWiki.Today());
        var pagesViewsStats =
            updatedStorage.Select(s => s.PagesStats(storageDaySpan).WhereStats(statsFilter));
        return pagesViewsStats;
    }
}