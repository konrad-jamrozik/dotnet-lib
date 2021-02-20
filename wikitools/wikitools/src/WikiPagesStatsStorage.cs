﻿using System;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;

namespace Wikitools
{
    public record WikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateTime CurrentDate)
    {
        public async Task<WikiPagesStatsStorage> Update(AdoWiki wiki, int pageViewsForDays)
        {
            var pageStats = await wiki.PagesStats(pageViewsForDays);

            var (previousMonthStats, currentMonthStats) = ValidWikiPagesStats.SplitByMonth(pageStats, CurrentDate);

            await Storage.With(CurrentDate,               (WikiPageStats[] stats) => ValidWikiPagesStats.From(stats).Merge(currentMonthStats).Value);
            await Storage.With(CurrentDate.AddMonths(-1), (WikiPageStats[] stats) => ValidWikiPagesStats.From(stats).Merge(previousMonthStats).Value);

            return this;
        }

        public ValidWikiPagesStats PagesStats(int pageViewsForDays)
        {
            var currentMonthDate = CurrentDate;
            var previousDate     = currentMonthDate.AddDays(-pageViewsForDays);
            var monthsDiffer     = previousDate.Month != currentMonthDate.Month;

            var currentMonthStats = new ValidWikiPagesStats(Storage.Read<WikiPageStats[]>(currentMonthDate));
            var previousMonthStats = new ValidWikiPagesStats(monthsDiffer
                ? Storage.Read<WikiPageStats[]>(previousDate)
                : new WikiPageStats[0]);

            // BUG 2 (already fixed, needs test) add filtering here to the pageViewsForDays, i.e. don't use all days of previous month.
            // Note that also the following case has to be tested for:
            //   the *current* (not previous) month needs to be filtered down.
            return previousMonthStats.Merge(currentMonthStats).Trim(previousDate, CurrentDate);
        }

        // kja make it a method on valid stats; probably same with Merge and Split
    }
}

// kja 4 high level next tasks
// Next tasks in Wikitools.WikiPagesStatsStorage.Update
// - deduplicate serialization logic with JsonDiff
// - replace File.WriteAllTextAsync. Introduce File abstraction or similar,
// that depends on OS.FileSystem. Make it create Dirs as needed when writing out.
//
// Later: think about decoupling the logic from FileSystem; maybe arbitrary storage via streams/writers
// would make more sense. At least the merging and splitting algorithm should be decoupled from file system.