using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps
{
    // kja need "Ado" prefix
    public record WikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateTime CurrentDate)
    {
        public async Task<WikiPagesStatsStorage> Update(IAdoWiki wiki, int pageViewsForDays)
        {
            var pageStats = await wiki.PagesStats(pageViewsForDays);

            var (previousMonthStats, currentMonthStats) = pageStats.SplitByMonth(CurrentDate);

            // kj3 add check here (or maybe inside Storage.With, need to think) that all storedStats are in the month of CurrentDate
            // It is necessary as I cannot trust the content of the storage: somebody might have
            // modified it.
            // Actually, I should also do check that stats are Valid
            await Storage.With<IEnumerable<WikiPageStats>>(CurrentDate.AddMonths(-1),
                storedStats => storedStats.Merge(previousMonthStats));
            await Storage.With<IEnumerable<WikiPageStats>>(CurrentDate,
                storedStats => storedStats.Merge(currentMonthStats));

            return this;
        }

        public async Task<WikiPagesStatsStorage> OverwriteWith(ValidWikiPagesStats stats, DateTime date)
        {
            // kja bug: doesn't delete previous month
            // kj3 add check here that the stats.month == date.month
            await Storage.With<IEnumerable<WikiPageStats>>(date, _ => stats);
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
