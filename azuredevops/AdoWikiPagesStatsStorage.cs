using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps;

public record AdoWikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateTime CurrentDate)
{
    public Task<AdoWikiPagesStatsStorage> Update(IAdoWiki wiki, PageViewsForDays pageViewsForDays) 
        => Update(pageViewsForDays, wiki.PagesStats);

    public Task<AdoWikiPagesStatsStorage> Update(
        IAdoWiki wiki,
        PageViewsForDays pageViewsForDays,
        int pageId)
        => Update(
            pageViewsForDays,
            pageViewsForDays => wiki.PageStats(pageViewsForDays, pageId));

    private async Task<AdoWikiPagesStatsStorage> Update(
        PageViewsForDays pageViewsForDays,
        Func<PageViewsForDays, Task<ValidWikiPagesStats>> wikiPagesStatsFunc)
    {
        var pagesStats = await wikiPagesStatsFunc(pageViewsForDays);

        var (previousMonthStats, currentMonthStats) = pagesStats.SplitIntoTwoMonths();
        if (previousMonthStats != null)
            await MergeIntoStoredMonthStats(previousMonthStats);
        await MergeIntoStoredMonthStats(currentMonthStats);

        return this;
    }

    private async Task MergeIntoStoredMonthStats(ValidWikiPagesStatsForMonth stats) =>
        await Storage.With<IEnumerable<WikiPageStats>>(stats.Month,
            storedStats =>
            {
                var validStoredStats = new ValidWikiPagesStatsForMonth(
                    storedStats,
                    stats.Month.FirstDay,
                    stats.EndDay);
                return new ValidWikiPagesStatsForMonth(validStoredStats.Merge(stats),
                    stats.Month.FirstDay,
                    stats.EndDay);
            });

    public async Task<AdoWikiPagesStatsStorage> ReplaceWith(ValidWikiPagesStats stats)
    {
        IEnumerable<ValidWikiPagesStatsForMonth> statsByMonth = stats.SplitByMonth();

        var writeTasks = statsByMonth
            .Select(async statsForMonth
                => await Storage.With<IEnumerable<WikiPageStats>>(
                    statsForMonth.Month, _ => statsForMonth));

        await Task.WhenAll(writeTasks);

        return this;
    }

    public ValidWikiPagesStats PagesStats(PageViewsForDays pageViewsForDays)
    {
        var currentDay = new DateDay(CurrentDate);
        // Here, the 1 is added to account for how ADO REST API interprets the range.
        // For more, see comment on:
        // AdoWikiWithStorageIntegrationTests.ObtainsAndStoresDataFromAdoWikiForToday
        var startDay = currentDay.AddDays(-pageViewsForDays.Value + 1);
            
        IEnumerable<ValidWikiPagesStatsForMonth> statsByMonth = DateMonth
            .Range(startDay, currentDay)
            .Select(month =>
            {
                var pageStats = Storage.Read<IEnumerable<WikiPageStats>>(month);
                return new ValidWikiPagesStatsForMonth(pageStats, month);
            });

        return ValidWikiPagesStats.Merge(statsByMonth).Trim(startDay, CurrentDate);
    }
}