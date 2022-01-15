﻿using System.Diagnostics.Contracts;
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
        var dayRange = pvfd.Value.MinWith(PageViewsForDaysMax);
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