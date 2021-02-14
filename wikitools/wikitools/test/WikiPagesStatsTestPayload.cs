using System;
using System.Linq;
using Wikitools.AzureDevOps;

namespace Wikitools.Tests
{
    // kja Days -> DayStats
    public record WikiPagesStatsTestPayload(
        DateTime Date,
        WikiPageStats.DayStat[] FooPagePreviousDayStats,
        WikiPageStats.DayStat[] FooPageCurrentDayStats,
        WikiPageStats.DayStat[] BarPagePreviousDayStats,
        WikiPageStats.DayStat[] BarPageCurrentDayStats,
        WikiPageStats.DayStat[][]? MergedDayStats = null,
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
                FooPage with { DayStats = MergedDayStats[0] },
                BarPage with { DayStats = MergedDayStats[1] }
            }
            : AllPagesStats;
    }
}