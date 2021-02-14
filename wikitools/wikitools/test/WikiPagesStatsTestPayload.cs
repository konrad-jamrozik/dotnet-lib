using System;
using System.Linq;
using Wikitools.AzureDevOps;

namespace Wikitools.Tests
{
    public record WikiPagesStatsTestPayload(
        DateTime Date,
        WikiPageStats.DayStat[] FooPagePreviousDays,
        WikiPageStats.DayStat[] FooPageCurrentDays,
        WikiPageStats.DayStat[] BarPagePreviousDays,
        WikiPageStats.DayStat[] BarPageCurrentDays,
        WikiPageStats.DayStat[][]? MergedDayStats = null,
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
            FooPage with { Stats = FooPagePreviousDays },
            BarPage with { Stats = BarPagePreviousDays }
        };

        public WikiPageStats[] CurrentMonth => new[]
        {
            FooPage with { Stats = FooPageCurrentDays },
            BarPage with { Stats = BarPageCurrentDays }
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