using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps;

public record AdoWikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateTime CurrentDate)
{
    public Task<AdoWikiPagesStatsStorage> Update(IAdoWiki wiki, PageViewsForDays pvfd) 
        => Update(pvfd, wiki.PagesStats);

    public Task<AdoWikiPagesStatsStorage> Update(
        IAdoWiki wiki,
        PageViewsForDays pvfd,
        int pageId)
        => Update(
            pvfd,
            pageViewsForDays => wiki.PageStats(pageViewsForDays, pageId)); // kj2 rename to pvfd

    // kj2 instead of taking pvfd, it could take Action() instead of Func().
    // That action will have pvfd already bound.
    private async Task<AdoWikiPagesStatsStorage> Update(
        PageViewsForDays pvfd,
        Func<PageViewsForDays, Task<ValidWikiPagesStats>> wikiPagesStatsFunc)
    {
        var pagesStats = await wikiPagesStatsFunc(pvfd);

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
                // kj2-DaySpan can the inputs to DaySpan here be simplified?
                var daySpan = new DaySpan(stats.Month.FirstDay, stats.DaySpan.EndDay);
                var validStoredStats = new ValidWikiPagesStatsForMonth(storedStats, daySpan);
                return new ValidWikiPagesStatsForMonth(validStoredStats.Merge(stats), daySpan);
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

    public ValidWikiPagesStats PagesStats(PageViewsForDays pvfd)
    {
        var currentDay = new DateDay(CurrentDate);
        // kj2-DaySpan Here, the 1 is added to account for how ADO REST API interprets the range.
        // For more, see comment on:
        // AdoWikiWithStorageIntegrationTests.ObtainsAndStoresDataFromAdoWikiForToday
        var startDay = currentDay.AddDays(-pvfd.Value + 1);
            
        IEnumerable<ValidWikiPagesStatsForMonth> statsByMonth = DateMonth
            .Span(startDay, currentDay)
            .Select(month =>
            {
                var pageStats = Storage.Read<IEnumerable<WikiPageStats>>(month);
                return new ValidWikiPagesStatsForMonth(pageStats, month);
            });

        return ValidWikiPagesStats.Merge(statsByMonth).Trim(startDay, CurrentDate);
    }
}