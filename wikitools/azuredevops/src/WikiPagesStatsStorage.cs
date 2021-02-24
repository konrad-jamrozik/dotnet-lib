using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib;

namespace Wikitools.AzureDevOps
{
    public record WikiPagesStatsStorage(MonthlyJsonFilesStorage Storage, DateTime CurrentDate)
    {
        public async Task<WikiPagesStatsStorage> Update(IAdoWiki wiki, int pageViewsForDays)
        {
            var pageStats = await wiki.PagesStats(pageViewsForDays);

            var (previousMonthStats, currentMonthStats) = pageStats.SplitByMonth(CurrentDate);

            await Storage.With(CurrentDate.AddMonths(-1),
                (IEnumerable<WikiPageStats> storedStats) => storedStats.Merge(previousMonthStats));
            await Storage.With(CurrentDate,
                (IEnumerable<WikiPageStats> storedStats) => storedStats.Merge(currentMonthStats));

            return this;
        }

        public ValidWikiPagesStats PagesStats(int pageViewsForDays)
        {
            var currentMonthDate = CurrentDate;
            var previousDate     = currentMonthDate.AddDays(-pageViewsForDays);
            var monthsDiffer     = previousDate.Month != currentMonthDate.Month;

            // kja do I need to ensure that FixNulls is called after Read here, before ctor is called?
            var currentMonthStats = new ValidWikiPagesStats(Storage.Read<WikiPageStats[]>(currentMonthDate));
            var previousMonthStats = new ValidWikiPagesStats(monthsDiffer
                ? Storage.Read<WikiPageStats[]>(previousDate)
                : new WikiPageStats[0]);

            return previousMonthStats.Merge(currentMonthStats).Trim(previousDate, CurrentDate);
        }
    }
}

// kja 4 high level next tasks
// Next tasks in Wikitools.WikiPagesStatsStorage.Update
// - deduplicate serialization logic with JsonDiff
// - replace File.WriteAllTextAsync. Introduce File abstraction or similar,
// that depends on OS.FileSystem. Make it create Dirs as needed when writing out.
//
// Later: think about decoupling the logic from FileSystem; maybe arbitrary storage via streams/writers
// would make more sense.