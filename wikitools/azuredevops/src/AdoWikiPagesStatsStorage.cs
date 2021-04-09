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

        public async Task<AdoWikiPagesStatsStorage> Update(IAdoWiki wiki, int pageViewsForDays)
        {
            var pageStats = await wiki.PagesStats(pageViewsForDays);

            var (previousMonthStats, currentMonthStats) = pageStats.SplitByMonth(CurrentDate);
            // kja ValidStatsForMonth simplify once SplitByMonth is well typed
            var previousMonth = CurrentMonth.AddMonths(-1);
            await MergeIntoStoredMonthStats(previousMonth, new ValidWikiPagesStatsForMonth(previousMonthStats, previousMonth));
            await MergeIntoStoredMonthStats(CurrentMonth, new ValidWikiPagesStatsForMonth(currentMonthStats, CurrentMonth));

            return this;
        }

        private async Task MergeIntoStoredMonthStats(DateMonth month, ValidWikiPagesStatsForMonth stats)
        {
            await Storage.With<IEnumerable<WikiPageStats>>(month,
                storedStats =>
                {
                    var validStoredStats = new ValidWikiPagesStatsForMonth(storedStats, month);
                    return validStoredStats.Merge(stats);
                });
        }

        // kja pass correct types as param instead of ctoring here
        public async Task<AdoWikiPagesStatsStorage> OverwriteWith(ValidWikiPagesStats stats, DateTime date)
        {
            // kja doesn't ensure that all relevant storage months are cleared up, only the current one
            // Instead, it should clear up all months present in the stats.
            // The current bad cleanup will influence what tests like this one test:
            // Wikitools.AzureDevOps.Tests.AdoWikiWithStorageIntegrationTests.ObtainsAndMergesDataFromAdoWikiApiAndStorage
            // I.e. sometimes it will work against data from previous test runs.

            var month       = new DateMonth(date);
            var statsToSave = new ValidWikiPagesStatsForMonth(stats, month);
            await Storage.With<IEnumerable<WikiPageStats>>(month, _ => statsToSave);
            return this;
        }

        public async Task<AdoWikiPagesStatsStorage> OverwriteWith(ValidWikiPagesStatsForMonth stats)
        {
            await Storage.With<IEnumerable<WikiPageStats>>(stats.Month, _ => stats);
            return this;
        }

        public ValidWikiPagesStats PagesStats(int pageViewsForDays)
        {
            var currentMonthDate = CurrentDate;
            var previousDate     = currentMonthDate.AddDays(-pageViewsForDays+1);
            var monthsDiffer     = previousDate.Month != currentMonthDate.Month;

            var currentMonthStats = new ValidWikiPagesStats(
                Storage.Read<WikiPageStats[]>(currentMonthDate));
            var previousMonthStats = new ValidWikiPagesStats(
                monthsDiffer
                    ? Storage.Read<WikiPageStats[]>(previousDate)
                    : new WikiPageStats[0]);

            return previousMonthStats.Merge(currentMonthStats).Trim(previousDate, CurrentDate);
        }
    }
}
