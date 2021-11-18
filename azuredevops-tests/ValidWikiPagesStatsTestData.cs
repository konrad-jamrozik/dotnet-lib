using System;
using System.Linq;
using Wikitools.Lib.Primitives;

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

        public ValidWikiPagesStatsForMonth PreviousMonthToMerge
        {
            get
            {
                WikiPageStats[] pageStats =
                    new[] { _fooPagePreviousMonth }
                        .Union(
                            !BarPageDeletedInPreviousMonth
                                ? new[] { _barPagePreviousMonth }
                                : WikiPageStats.EmptyArray)
                        .ToArray();

                var stats = ValidWikiPagesStatsFixture.BuildForMonth(pageStats);
                if (stats.AnyDayVisitsPresent)
                {
                    // Extend the day span to last day of month to avoid violating constraint
                    // forbidding having day span gap when
                    // merging this (PreviousMonthToMerge) month with month CurrentMonthToMerge.
                    var statsWithDaySpanExtendedToMonthEnd = new ValidWikiPagesStatsForMonth(
                        new ValidWikiPagesStats(
                            stats,
                            stats.StartDay,
                            stats.Month.LastDay));
                    return statsWithDaySpanExtendedToMonthEnd;
                }
                else
                    return stats;
            }
        }

        // This is separate from PreviousMonthToMerge due to analogous reasoning as explained
        // in comment for CurrentMonthAfterSplit.
        public ValidWikiPagesStatsForMonth PreviousMonthAfterSplit
        {
            get
            {
                WikiPageStats[] pageStats = { _fooPagePreviousMonth, _barPagePreviousMonth };
                return ValidWikiPagesStatsFixture.BuildForMonth(pageStats);
            }
        }

        // This is used for asserting resulting previous month from splitting AllPagesStats.
        // To understand why consider a situation in which Foo page path has changed
        // between previous and current month. In AllPagesStats Foo page will show
        // with the new path. Hence splitting it will show the new path in previous and current month,
        // even though previous-month-only stats like _fooPagePreviousMonth would show previous path.
        public ValidWikiPagesStatsForMonth PreviousMonthWithCurrentMonthPaths
        {
            get
            {
                WikiPageStats[] pageStats = {
                    _fooPagePreviousMonthWithCurrentMonthPath,
                    _barPagePreviousMonthWithCurrentMonthPath
                };
                return ValidWikiPagesStatsFixture.BuildForMonth(pageStats);
            }
        }

        public ValidWikiPagesStatsForMonth CurrentMonthToMerge
        {
            get
            {
                WikiPageStats[] pageStats = 
                    (!FooPageDeletedInCurrentMonth
                        ? new[] { _fooPageCurrentMonth }
                        : WikiPageStats.EmptyArray)
                    .Union(new[] { _barPageCurrentMonth })
                    .ToArray();

                var stats = ValidWikiPagesStatsFixture.BuildForMonth(pageStats);

                if (stats.AnyDayVisitsPresent)
                {
                    // Extend the day span to last day of month to avoid violating constraint
                    // forbidding having day span gap when
                    // merging PreviousMonthToMerge month with this month (CurrentMonthToMerge).
                    var statsWithDaySpanExtendedToMonthStart = new ValidWikiPagesStatsForMonth(
                        new ValidWikiPagesStats(
                            stats,
                            stats.Month.FirstDay,
                            stats.EndDay));
                    return statsWithDaySpanExtendedToMonthStart;
                }
                else 
                {
                    if (PreviousMonthToMerge.AnyDayVisitsPresent)
                    {
                        // This case is required to make test
                        // Wikitools.AzureDevOps.Tests.ValidWikiPagesStatsTests.PageStatsPreviousMonthOnly
                        // pass when exercising Merge.
                        // Without this, the current month would have day span of one day of new SimulatedTimeline().UtcNow
                        // and that most likely violate the invariant day span rules present in previous month.
                        var statsSpanDay = PreviousMonthToMerge.EndDay.AddDays(1);
                        return new ValidWikiPagesStatsForMonth(new ValidWikiPagesStats(WikiPageStats.EmptyArray,
                            statsSpanDay, statsSpanDay));
                    }
                    else
                        return stats;    
                    
                }
            }
        }

        // This comment uses data example from test
        // Wikitools.AzureDevOps.Tests.ValidWikiPagesStatsTestDataFixture.PageStatsPagesMissing
        //
        // This needs to be used for asserting current month after .SplitByMonth() on AllPagesStats,
        // instead of CurrentMonthToMerge, which keeps entry for a page that has no days, as opposed to this property.
        // We need to use this property because the splitting of AllPagesStats does not retain information about
        // the existence fooPage in current month if it has no stats. Having no stats can mean both:
        // "page exists but had no views this month"
        // and "page no longer exists"
        // thus the expectation here is that .SplitByMonth() will retain the page entry with no day
        // view stats, even if the page was deleted.
        // It is assumed that client code is responsible for post-processing of the data
        // to remove any deleted pages, if need be.
        //
        // For corresponding test case and comment on it, please see
        // ValidWikiPagesStatsTestDataFixture.PageStatsPagesMissing
        public ValidWikiPagesStatsForMonth CurrentMonthAfterSplit
        {
            get
            {
                WikiPageStats[] pageStats = { _fooPageCurrentMonth, _barPageCurrentMonth };
                return ValidWikiPagesStatsFixture.BuildForMonth(pageStats);
            }
        }

        public ValidWikiPagesStats AllPagesStats
        {
            get
            {
                WikiPageStats[] pageStats = { _fooPage, _barPage };
                return ValidWikiPagesStatsFixture.Build(pageStats, currentDay: new DateDay(Date));
            }
        }

        public ValidWikiPagesStats MergedPagesStats
        {
            get
            {
                return MergedDayStats != null
                    ? ValidWikiPagesStatsFixture.Build(new[] { FooPageMerged, BarPageMerged })
                    : AllPagesStats;
            }
        }
    }
}