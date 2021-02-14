using System;
using System.Linq;
using Wikitools.AzureDevOps;

namespace Wikitools.Tests
{
    // kja Days -> DayStats
    public record WikiPagesStatsTestPayload(
        DateTime Date,
        WikiPageStats.DayStat[] FooPagePreviousDays,
        WikiPageStats.DayStat[] FooPageCurrentDays,
        WikiPageStats.DayStat[] BarPagePreviousDays,
        WikiPageStats.DayStat[] BarPageCurrentDays,
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
            FooPagePreviousDays.Concat(FooPageCurrentDays).ToArray());

        private WikiPageStats BarPage => new(BarPagePath,
            BarPageId,
            BarPagePreviousDays.Concat(BarPageCurrentDays).ToArray());

        public WikiPageStats[] PreviousMonth => new[]
        {
            FooPage with { Stats = FooPagePreviousMonthDayStats ?? FooPagePreviousDays },
            BarPage with { Stats = BarPagePreviousMonthDayStats ?? BarPagePreviousDays }
        };

        public WikiPageStats[] CurrentMonth => new[]
        {
            FooPage with { Stats = FooPageCurrentMonthDayStats ?? FooPageCurrentDays },
            BarPage with { Stats = BarPageCurrentMonthDayStats ?? BarPageCurrentDays }
        };

        public WikiPageStats[] AllPagesStats => new[] { FooPage, BarPage };

        public WikiPageStats[] MergedPagesStats => MergedDayStats != null
            ? new[]
            {
                FooPage with { Stats = MergedDayStats[0] },
                BarPage with { Stats = MergedDayStats[1] }
            }
            : AllPagesStats;
    }
}