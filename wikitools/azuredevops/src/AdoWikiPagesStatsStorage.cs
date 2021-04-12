using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps
{
    public record AdoWikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateTime CurrentDate)
    {
        private DateMonth CurrentMonth => new(CurrentDate);

        public Task<AdoWikiPagesStatsStorage> Update(IAdoWiki wiki, int pageViewsForDays) =>
            Update(pageViewsForDays, wiki.PagesStats);

        public Task<AdoWikiPagesStatsStorage> Update(IAdoWiki wiki, int pageViewsForDays, int pageId) => Update(
            pageViewsForDays,
            pageViewsForDays => wiki.PageStats(pageViewsForDays, pageId));

        private async Task<AdoWikiPagesStatsStorage> Update(
            int pageViewsForDays,
            Func<int, Task<ValidWikiPagesStats>> wikiPageStatsFunc)
        {
            var pageStats = await wikiPageStatsFunc(pageViewsForDays);

            var (previousMonthStats, currentMonthStats) = pageStats.SplitByMonth(CurrentMonth);
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

        public async Task<AdoWikiPagesStatsStorage> OverwriteWith(ValidWikiPagesStats stats, DateTime date)
        {
            var (previousMonthStats, currentMonthStats) = stats.SplitByMonth(new DateMonth(date));
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
            // kja bug: this will return equal on difference of exactly 12 months, but should still say that months differ
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
