﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps
{
    public record WikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateTime CurrentDate)
    {
        public async Task<WikiPagesStatsStorage> Update(IAdoWikiApi wiki, int pageViewsForDays)
        {
            var pageStats = await wiki.PagesStats(pageViewsForDays);

            var (previousMonthStats, currentMonthStats) = pageStats.SplitByMonth(CurrentDate);

            // kja causes test AdoWikiWithStorageTests.NoData to fail, as it doesn't detect there is nothing to write.
            await Storage.With<IEnumerable<WikiPageStats>>(CurrentDate.AddMonths(-1),
                storedStats => storedStats.Merge(previousMonthStats));
            await Storage.With<IEnumerable<WikiPageStats>>(CurrentDate,
                storedStats => storedStats.Merge(currentMonthStats));

            return this;
        }

        public async Task<WikiPagesStatsStorage> DeleteExistingAndSave(ValidWikiPagesStats stats, DateTime date)
        {
            await Storage.With<IEnumerable<WikiPageStats>>(date, _ => stats);
            return this;
        }

        public ValidWikiPagesStats PagesStats(int pageViewsForDays)
        {
            var currentMonthDate = CurrentDate;
            var previousDate     = currentMonthDate.AddDays(-pageViewsForDays);
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
