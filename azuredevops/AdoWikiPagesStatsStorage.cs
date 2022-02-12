using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps;

/// <summary>
/// Represents a MonthlyJsonFilesStorage that stores stats data originating from IAdoWiki.
/// i.e. the data of type ValidWikiPagesStats.
///
/// For a class abstracting IAdoWiki backed by this storage, see AdoWikiWithStorage.
/// </summary>
public record AdoWikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateDay CurrentDay)
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
        var daySpan = pvfd.AsDaySpanUntil(CurrentDay);
        IEnumerable<ValidWikiPagesStatsForMonth> statsByMonth = DateMonth
            .MonthSpan(daySpan)
            .Select(month =>
            {
                var pageStats = Storage.Read<IEnumerable<WikiPageStats>>(month);
                return new ValidWikiPagesStatsForMonth(pageStats, month);
            });

        return ValidWikiPagesStats.Merge(statsByMonth).Trim(daySpan);
    }
}