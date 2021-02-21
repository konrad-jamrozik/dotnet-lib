using System;
using System.Linq;

namespace Wikitools.AzureDevOps.Tests
{
    /// <summary>
    /// Data for testing method processing on WikiPageStats.
    ///
    /// Composed of four parts, that can be visualized as cells of 2-dimensional array of:
    /// X-axis: (previousMonth, currentMonth)
    /// Y-axis: (fooPage, barPage).
    /// An example of one of the 4 cells: (previousMonth, fooPage).
    /// Each cell is a WikiPageStats object, and thus also contains DayStat[] array.
    /// The DayStat arrays are passed as ctor param, with the remaining WikiPageStats data
    /// provided by default, but also overridable.
    /// </summary>
    public record ValidWikiPagesStatsTestData(
        DateTime Date,
        WikiPageStats.DayStat[] FooPagePreviousDayStats,
        WikiPageStats.DayStat[] FooPageCurrentDayStats,
        WikiPageStats.DayStat[] BarPagePreviousDayStats,
        WikiPageStats.DayStat[] BarPageCurrentDayStats,
        bool SplitPreconditionsViolated = false,
        (WikiPageStats.DayStat[] FooPage, WikiPageStats.DayStat[] BarPage)? MergedDayStats = null,
        string? FooPagePathInCurrentMonth = null,
        string? BarPagePathInCurrentMonth = null)
    {
        public const string FooPagePath = "/Foo";
        public const string BarPagePath = "/Bar";
        private static int FooPageId = 101;
        private static int BarPageId = 202;

        // @formatter:off
        private readonly WikiPageStats _fooPagePreviousMonth = new(FooPagePath, FooPageId, FooPagePreviousDayStats);
        private readonly WikiPageStats _barPagePreviousMonth = new(BarPagePath, BarPageId, BarPagePreviousDayStats);
        private readonly WikiPageStats _fooPagePreviousMonthWithCurrentMonthPath = new(FooPagePathInCurrentMonth ?? FooPagePath, FooPageId, FooPagePreviousDayStats);
        private readonly WikiPageStats _barPagePreviousMonthWithCurrentMonthPath = new(BarPagePathInCurrentMonth ?? BarPagePath, BarPageId, BarPagePreviousDayStats);
        private readonly WikiPageStats _fooPageCurrentMonth = new(FooPagePathInCurrentMonth ?? FooPagePath, FooPageId, FooPageCurrentDayStats);
        private readonly WikiPageStats _barPageCurrentMonth = new(BarPagePathInCurrentMonth ?? BarPagePath, BarPageId, BarPageCurrentDayStats);
        private readonly WikiPageStats _fooPage = new(FooPagePathInCurrentMonth ?? FooPagePath, FooPageId, FooPagePreviousDayStats.Concat(FooPageCurrentDayStats).ToArray());
        private readonly WikiPageStats _barPage = new(BarPagePathInCurrentMonth ?? BarPagePath, BarPageId, BarPagePreviousDayStats.Concat(BarPageCurrentDayStats).ToArray());
        private WikiPageStats FooPageMerged => new(FooPagePathInCurrentMonth ?? FooPagePath, FooPageId, MergedDayStats!.Value.FooPage);
        private WikiPageStats BarPageMerged => new(BarPagePathInCurrentMonth ?? BarPagePath, BarPageId, MergedDayStats!.Value.BarPage);
        // @formatter:on

        public readonly bool PageRenamePresent = FooPagePathInCurrentMonth != null || BarPagePathInCurrentMonth != null;

        public ValidWikiPagesStats PreviousMonth => new(new[] { _fooPagePreviousMonth, _barPagePreviousMonth });

        public ValidWikiPagesStats PreviousMonthWithCurrentMonthPaths => new(new[]
        {
            _fooPagePreviousMonthWithCurrentMonthPath,
            _barPagePreviousMonthWithCurrentMonthPath
        });

        public ValidWikiPagesStats CurrentMonth => new(new[] { _fooPageCurrentMonth, _barPageCurrentMonth });
        public ValidWikiPagesStats AllPagesStats => new(new[] { _fooPage, _barPage });

        public ValidWikiPagesStats MergedPagesStats => MergedDayStats != null
            ? new ValidWikiPagesStats(new[] { FooPageMerged, BarPageMerged })
            : AllPagesStats;
    }
}