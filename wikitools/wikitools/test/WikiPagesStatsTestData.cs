using System;
using System.Linq;
using Wikitools.AzureDevOps;

namespace Wikitools.Tests
{
    /// <summary>
    /// Data for testing method processing on WikiPageStats.
    ///
    /// Composed of four parts, that can be visualized as cells of 2-dimensional array of:
    /// X-axis: (previousMonth, currentMonth)
    /// Y-axis: (fooPage, barPage).
    /// An example of one of the 4 cells is thus (fooPage for previousMonth).
    /// Each cell is a WikiPageStats object, and thus also contains DayStat[] array.
    /// The DayStat arrays are passed as ctor param, with the remaining WikiPageStats data
    /// provided by default, but also overridable.
    /// </summary>
    public record WikiPagesStatsTestData(
        DateTime Date,
        WikiPageStats.DayStat[] FooPagePreviousDayStats,
        WikiPageStats.DayStat[] FooPageCurrentDayStats,
        WikiPageStats.DayStat[] BarPagePreviousDayStats,
        WikiPageStats.DayStat[] BarPageCurrentDayStats,
        bool SplitPreconditionsViolated = false,
        WikiPageStats.DayStat[]? FooPagePreviousDayStatsToMerge = null,
        WikiPageStats.DayStat[]? BarPagePreviousDayStatsToMerge = null,
        (WikiPageStats.DayStat[] FooPage, WikiPageStats.DayStat[] BarPage)? MergedDayStats = null,
        string? FooPagePathInCurrentMonth = null,
        string? BarPagePathInCurrentMonth = null)
    {

        private const string FooPagePath = "/Foo";
        private const string BarPagePath = "/Bar";

        // kja once the specced out invariants are enforced by the type, this fixture will adapt (otherwise it wouldn't compile)
        // making things work.
        //
        // currMonth = fooPage(currMonthDayStats), barPage(currMonthDayStats)
        // previousMonth = fooPage(prevMonthDayStats), barPage(prevMonthDayStats)
        // allStats = fooPage(prevMonthDayStats ++ currMonthDayStats), barPage(prevMonthDayStats ++ currMonthDayStats)
        // (previousMonth, currentMonth) = split(AllStats)
        // merged = merge(previousMonth, currentMonth)

        private WikiPageStats FooPage => new(FooPagePath,
            101,
            FooPagePreviousDayStats.Concat(FooPageCurrentDayStats).ToArray());

        private WikiPageStats BarPage => new(BarPagePath,
            202,
            BarPagePreviousDayStats.Concat(BarPageCurrentDayStats).ToArray());

        public readonly bool PageRenamePresent = FooPagePathInCurrentMonth != null || BarPagePathInCurrentMonth != null;

        public ValidWikiPagesStats PreviousMonth => new(new[]
        {
            FooPage with { Path = FooPagePath, DayStats = FooPagePreviousDayStats },
            BarPage with { Path = BarPagePath, DayStats = BarPagePreviousDayStats }
        });

        // kja replace all the 'with' with direct ctor calls
        public ValidWikiPagesStats PreviousMonthAfterSplit => new(new[]
        {
            FooPage with { Path = FooPagePathInCurrentMonth ?? FooPagePath, DayStats = FooPagePreviousDayStats },
            BarPage with { Path = BarPagePathInCurrentMonth ?? BarPagePath, DayStats = BarPagePreviousDayStats }
        });

        public ValidWikiPagesStats PreviousMonthAfterMerge => new(new[]
        {
            FooPage with { Path = FooPagePathInCurrentMonth ?? FooPagePath, DayStats = FooPagePreviousDayStatsToMerge ?? FooPagePreviousDayStats },
            BarPage with { Path = BarPagePathInCurrentMonth ?? BarPagePath, DayStats = BarPagePreviousDayStatsToMerge ?? BarPagePreviousDayStats }
        });

        public ValidWikiPagesStats CurrentMonth => new(new[]
        {
            FooPage with { Path = FooPagePathInCurrentMonth ?? FooPagePath, DayStats = FooPageCurrentDayStats },
            BarPage with { Path = BarPagePathInCurrentMonth ?? BarPagePath, DayStats = BarPageCurrentDayStats }
        });

        public ValidWikiPagesStats AllPagesStats => new(new[]
        {
            FooPage with { Path = FooPagePathInCurrentMonth ?? FooPagePath }, 
            BarPage with { Path = BarPagePathInCurrentMonth ?? BarPagePath }
        });

        public ValidWikiPagesStats PreviousStatsToMerge => new(new[]
        {
            FooPage with { Path = FooPagePath, DayStats = FooPagePreviousDayStatsToMerge ?? FooPagePreviousDayStats },
            BarPage with { Path = BarPagePath, DayStats = BarPagePreviousDayStatsToMerge ?? BarPagePreviousDayStats }
        });

        public ValidWikiPagesStats MergedPagesStats => MergedDayStats != null
            ? new ValidWikiPagesStats(new[]
            {
                FooPage with { Path = FooPagePathInCurrentMonth ?? FooPagePath, DayStats = MergedDayStats!.Value.FooPage },
                BarPage with { Path = BarPagePathInCurrentMonth ?? BarPagePath, DayStats = MergedDayStats!.Value.BarPage }
            })
            : AllPagesStats;
    }


}