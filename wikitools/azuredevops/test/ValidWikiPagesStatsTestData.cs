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
        WikiPageStats.DayStat[] FooPagePreviousMonthDayStats,
        WikiPageStats.DayStat[] FooPageCurrentMonthDayStats,
        WikiPageStats.DayStat[] BarPagePreviousMonthDayStats,
        WikiPageStats.DayStat[] BarPageCurrentMonthDayStats,
        bool SplitPreconditionsViolated = false,
        (WikiPageStats.DayStat[] FooPage, WikiPageStats.DayStat[] BarPage)? MergedDayStats = null,
        string? FooPagePathInCurrentMonth = null,
        string? BarPagePathInCurrentMonth = null,
        bool FooPageDeletedInCurrentMonth = false,
        bool BarPageDeletedInPreviousMonth = false)
    {
        public const string FooPagePath = "/Foo";
        public const string BarPagePath = "/Bar";
        private const int FooPageId = 101;
        private const int BarPageId = 202;

        // @formatter:off
        private readonly WikiPageStats _fooPagePreviousMonth = new(FooPagePath, FooPageId, FooPagePreviousMonthDayStats);
        private readonly WikiPageStats _barPagePreviousMonth = new(BarPagePath, BarPageId, BarPagePreviousMonthDayStats);
        private readonly WikiPageStats _fooPagePreviousMonthWithCurrentMonthPath = new(FooPagePathInCurrentMonth ?? FooPagePath, FooPageId, FooPagePreviousMonthDayStats);
        private readonly WikiPageStats _barPagePreviousMonthWithCurrentMonthPath = new(BarPagePathInCurrentMonth ?? BarPagePath, BarPageId, BarPagePreviousMonthDayStats);
        private readonly WikiPageStats _fooPageCurrentMonth = new(FooPagePathInCurrentMonth ?? FooPagePath, FooPageId, FooPageCurrentMonthDayStats);
        private readonly WikiPageStats _barPageCurrentMonth = new(BarPagePathInCurrentMonth ?? BarPagePath, BarPageId, BarPageCurrentMonthDayStats);
        private readonly WikiPageStats _fooPage = new(FooPagePathInCurrentMonth ?? FooPagePath, FooPageId, FooPagePreviousMonthDayStats.Concat(FooPageCurrentMonthDayStats).ToArray());
        private readonly WikiPageStats _barPage = new(BarPagePathInCurrentMonth ?? BarPagePath, BarPageId, BarPagePreviousMonthDayStats.Concat(BarPageCurrentMonthDayStats).ToArray());
        private WikiPageStats FooPageMerged => new(FooPagePathInCurrentMonth ?? FooPagePath, FooPageId, MergedDayStats!.Value.FooPage);
        private WikiPageStats BarPageMerged => new(BarPagePathInCurrentMonth ?? BarPagePath, BarPageId, MergedDayStats!.Value.BarPage);
        // @formatter:on

        public readonly bool PageRenamePresent = FooPagePathInCurrentMonth != null || BarPagePathInCurrentMonth != null;

        public ValidWikiPagesStats PreviousMonthToMerge =>
            new(new[] { _fooPagePreviousMonth }
                .Union(
                    !BarPageDeletedInPreviousMonth
                        ? new[] { _barPagePreviousMonth }
                        : WikiPageStats.EmptyArray)
                .ToArray());

        // This is separate from PreviousMonthToMerge due to analogous reasoning as explained
        // in comment for CurrentMonthAfterSplit.
        public ValidWikiPagesStats PreviousMonthAfterSplit =>
            new(new[] { _fooPagePreviousMonth, _barPagePreviousMonth });

        public ValidWikiPagesStats PreviousMonthWithCurrentMonthPaths => new(new[]
        {
            _fooPagePreviousMonthWithCurrentMonthPath,
            _barPagePreviousMonthWithCurrentMonthPath
        });

        public ValidWikiPagesStats CurrentMonthToMerge =>             
            new((!FooPageDeletedInCurrentMonth 
                    ? new[] { _fooPageCurrentMonth } 
                    : WikiPageStats.EmptyArray)
                .Union(new [] {_barPageCurrentMonth})
                .ToArray());

        // This needs to be used for asserting current month after .SplitByMonth() on AllPagesStats,
        // instead of CurrentMonthToMerge. This is because AllPagesStats does not retain information about
        // the fooPage in current month. It has no stats. This can mean both:
        // "page exists but had no views this month"
        // and "page no longer exists"
        // thus the expectation here is that .SplitByMonth will retain the page entry with no day
        // view stats, even if the page was deleted. It is assumed post-processing of the data
        // will remove any deleted pages, if need be.
        //
        // For corresponding test case and comment on it, please see
        // ValidWikiPagesStatsTestDataFixture.PageStatsPagesMissing
        public ValidWikiPagesStats CurrentMonthAfterSplit =>
            new(new[] { _fooPageCurrentMonth, _barPageCurrentMonth });

        public ValidWikiPagesStats AllPagesStats => new(new[] { _fooPage, _barPage });

        public ValidWikiPagesStats MergedPagesStats => MergedDayStats != null
            ? new ValidWikiPagesStats(new[] { FooPageMerged, BarPageMerged })
            : AllPagesStats;
    }
}