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
        (WikiPageStats.DayStat[] FooPage, WikiPageStats.DayStat[] BarPage)? MergedDayStats = null,
        WikiPageStats.DayStat[]? FooPagePreviousMonthDayStats = null,
        WikiPageStats.DayStat[]? FooPageCurrentMonthDayStats = null,
        WikiPageStats.DayStat[]? BarPagePreviousMonthDayStats = null,
        WikiPageStats.DayStat[]? BarPageCurrentMonthDayStats = null,
        bool SplitByMonthThrows = false,
        bool MergeThrows = false,
        string FooPagePath = "/Foo",
        string BarPagePath = "/Bar",
        int FooPageId = 100,
        int BarPageId = 200)
    {
        // kja once the specced out invariants are enforced by the type, this fixture will adapt (otherwise it wouldn't compile)
        // making things work.
        //
        // currMonth = fooPage(currMonthDayStats), barPage(currMonthDayStats)
        // previousMonth = fooPage(prevMonthDayStats), barPage(prevMonthDayStats)
        // allStats = fooPage(prevMonthDayStats ++ currMonthDayStats), barPage(prevMonthDayStats ++ currMonthDayStats)
        // (previousMonth, currentMonth) = split(AllStats)
        // merged = merge(previousMonth, currentMonth)

        private WikiPageStats FooPage => new(FooPagePath,
            FooPageId,
            FooPagePreviousDayStats.Concat(FooPageCurrentDayStats).ToArray());

        private WikiPageStats BarPage => new(BarPagePath,
            BarPageId,
            BarPagePreviousDayStats.Concat(BarPageCurrentDayStats).ToArray());

        public WikiPageStats[] PreviousMonth => new[]
        {
            FooPage with { DayStats = FooPagePreviousMonthDayStats ?? FooPagePreviousDayStats },
            BarPage with { DayStats = BarPagePreviousMonthDayStats ?? BarPagePreviousDayStats }
        };

        public WikiPageStats[] CurrentMonth => new[]
        {
            FooPage with { DayStats = FooPageCurrentMonthDayStats ?? FooPageCurrentDayStats },
            BarPage with { DayStats = BarPageCurrentMonthDayStats ?? BarPageCurrentDayStats }
        };

        public WikiPageStats[] AllPagesStats => new[] { FooPage, BarPage };

        public WikiPageStats[] MergedPagesStats => MergedDayStats != null
            ? new[]
            {
                FooPage with { DayStats = MergedDayStats!.Value.FooPage },
                BarPage with { DayStats = MergedDayStats!.Value.BarPage }
            }
            : AllPagesStats;
    }


}