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
        private DateMonth CurrentMonth => new(CurrentDate);

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

            var (previousMonthStats, currentMonthStats) = pagesStats.SplitIntoTwoMonths(CurrentMonth);
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
                        statsForMonth.Month, _ => stats));

            await Task.WhenAll(writeTasks);

            return this;
        }

        public async Task<AdoWikiPagesStatsStorage> OverwriteWithTwoMonths(ValidWikiPagesStats stats)
        {
            var (previousMonthStats, currentMonthStats) = stats.SplitIntoTwoMonths(CurrentMonth);
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
            var currentDay   = new DateDay(CurrentDate);
            var previousDate = currentDay.AddDays(-pageViewsForDays + 1);
            // kj2 bug: this will return equal on difference of exactly 12 months, but should still say that months differ
            // Note that the code below assumes it can go max in the past by one month only
            var monthsDiffer = previousDate.Month != currentDay.Month;

            var currentMonthStats = new ValidWikiPagesStats(
                Storage.Read<IEnumerable<WikiPageStats>>(currentDay));
            var previousMonthStats = new ValidWikiPagesStats(
                monthsDiffer
                    ? Storage.Read<IEnumerable<WikiPageStats>>(previousDate)
                    : new WikiPageStats[0]);

            return previousMonthStats.Merge(currentMonthStats).Trim(previousDate, CurrentDate);
        }
    }
}
