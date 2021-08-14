using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps
{
    public record AdoWikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateTime CurrentDate)
    {
        public Task<AdoWikiPagesStatsStorage> Update(IAdoWiki wiki, int pageViewsForDays) 
            => Update(pageViewsForDays, wiki.PagesStats);

        public Task<AdoWikiPagesStatsStorage> Update(IAdoWiki wiki, int pageViewsForDays, int pageId)
            => Update(
                pageViewsForDays,
                pageViewsForDays => wiki.PageStats(pageViewsForDays, pageId));

        private async Task<AdoWikiPagesStatsStorage> Update(
            int pageViewsForDays,
            Func<int, Task<ValidWikiPagesStats>> wikiPagesStatsFunc)
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
                    var validStoredStats = new ValidWikiPagesStatsForMonth(storedStats, stats.Month);
                    return new ValidWikiPagesStatsForMonth(validStoredStats.Merge(stats), stats.Month);
                });

        public async Task<AdoWikiPagesStatsStorage> OverwriteWith(ValidWikiPagesStats stats)
        {
            if (stats is ValidWikiPagesStatsForMonth statsForMonth)
            {
                return await OverwriteWith(statsForMonth);
            }

            IEnumerable<ValidWikiPagesStatsForMonth> statsByMonth = stats.SplitByMonth();

            var writeTasks = statsByMonth
                .Select(async statsForMonth
                    => await Storage.With<IEnumerable<WikiPageStats>>(
                        statsForMonth.Month, _ => statsForMonth));

            await Task.WhenAll(writeTasks);

            return this;
        }

        public async Task<AdoWikiPagesStatsStorage> OverwriteWithTwoMonths(ValidWikiPagesStats stats)
        {
            var (previousMonthStats, currentMonthStats) = stats.SplitIntoTwoMonths();
            if (previousMonthStats != null) 
                await Storage.With<IEnumerable<WikiPageStats>>(previousMonthStats.Month, _ => previousMonthStats);
            await Storage.With<IEnumerable<WikiPageStats>>(currentMonthStats.Month, _ => currentMonthStats);
            return this;
        }

        public async Task<AdoWikiPagesStatsStorage> OverwriteWith(ValidWikiPagesStatsForMonth stats)
        {
            await Storage.With<IEnumerable<WikiPageStats>>(stats.Month, _ => stats);
            return this;
        }

        public ValidWikiPagesStats PagesStats(int pageViewsForDays)
        {
            var currentDay = new DateDay(CurrentDate);
            var startDay = currentDay.AddDays(-pageViewsForDays + 1);
            
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
}
