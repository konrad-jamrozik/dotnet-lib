using System;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps.Tests;

public static class ValidWikiPagesStatsTestDataFixture
{
        // @formatter:off
        private static readonly DateDay DecemberDay = new DateDay(2020, 12, 22, DateTimeKind.Utc);
        private static readonly DateDay  JanuaryDay = new DateDay(2021,  1,  3, DateTimeKind.Utc);
        private static readonly DateDay FebruaryDay = new DateDay(2021,  2, 15, DateTimeKind.Utc);
        // @formatter:on

    public static ValidWikiPagesStatsTestData PageStatsEmpty =>
        new(FebruaryDay,
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { });

    public static ValidWikiPagesStatsTestData PageStatsPreviousMonthOnly =>
        new(FebruaryDay,
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { new(115, FebruaryDay.AddMonths(-1)) },
            new WikiPageStats.DayStat[] { });

    public static ValidWikiPagesStatsTestData PageStatsYearWrap =>
        new(JanuaryDay,
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { new(1201, JanuaryDay.AddMonths(-1).AddDays(-2)) },
            new WikiPageStats.DayStat[] { new(103, JanuaryDay) });

    public static ValidWikiPagesStatsTestData PageStatsBeforeYearWrap =>
        new(DecemberDay,
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { new(1122, DecemberDay.AddMonths(-1)) },
            new WikiPageStats.DayStat[] { new(1223, DecemberDay.AddDays(1)) });

    public static ValidWikiPagesStatsTestData PageStatsRenamedToNewPath =>
        new(FebruaryDay,
            new WikiPageStats.DayStat[] { new(103, JanuaryDay) },
            new WikiPageStats.DayStat[] { new(217, FebruaryDay.AddDays(2)) },
            new WikiPageStats.DayStat[] { new(104, JanuaryDay.AddDays(1)) },
            new WikiPageStats.DayStat[] { new(219, FebruaryDay.AddDays(4)) },
            FooPagePathInCurrentMonth: "/Qux"
        );

    public static ValidWikiPagesStatsTestData PageStatsExchangedPaths =>
        new(FebruaryDay,
            new WikiPageStats.DayStat[] { new(103, JanuaryDay) },
            new WikiPageStats.DayStat[] { new(217, FebruaryDay.AddDays(2)) },
            new WikiPageStats.DayStat[] { new(104, JanuaryDay.AddDays(1)) },
            new WikiPageStats.DayStat[] { new(219, FebruaryDay.AddDays(4)) },
            FooPagePathInCurrentMonth: ValidWikiPagesStatsTestData.BarPagePath,
            BarPagePathInCurrentMonth: ValidWikiPagesStatsTestData.FooPagePath
        );

    /// <summary>
    /// This test specifically aims to show how the tested class logic
    /// behaves on data that:
    /// - had a given Foo page deleted in the most recent month, thus resulting
    /// in the Foo page stats being completely absent from data returned
    /// from Wikitools.AzureDevOps.AdoWikiApi.GetAllWikiPagesDetails,
    /// even if there were day view stats for it in the queried days.
    /// - and had a given Bar page created in current month,
    /// but not having been present in previous month.
    ///
    /// The expected behavior is as follows:
    /// - when merging data for previous and current month, retain the day view
    /// stats for the page that was deleted, even though ADO would no longer show
    /// the view stats for that page in current month.
    /// - as a consequence, when calling .SplitByMonth() on merged data, always show the pages
    /// with empty day view stats, even if they didn't exist because they were deleted
    /// or weren't yet created. .SplitByMonth() is unable to determine if pages
    /// were nonexistent or just with no views, as merging doesn't retain this information.
    /// For details, please see comment on:
    /// ValidWikiPagesStatsTestData.CurrentMonthAfterSplit
    /// </summary>
    public static ValidWikiPagesStatsTestData PageStatsPagesMissing =>
        new(FebruaryDay,
            new WikiPageStats.DayStat[] { new(103, JanuaryDay) },
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { },
            new WikiPageStats.DayStat[] { new(219, FebruaryDay.AddDays(4)) },
            FooPageDeletedInCurrentMonth: true,
            BarPageDeletedInPreviousMonth: true
        );

    public static ValidWikiPagesStatsTestData PageStats
    {
        get
        {
            var fooDaysPreviousMonth = new WikiPageStats.DayStat[]
            {
                new(113, FebruaryDay.AddMonths(-1).AddDays(-2)),
                new(114, FebruaryDay.AddMonths(-1).AddDays(-1)),
                new(115, FebruaryDay.AddMonths(-1)),
                new(131, FebruaryDay.AddDays(-15))
            };

            var fooDaysCurrentMonth = new WikiPageStats.DayStat[]
            {
                new(201, FebruaryDay.AddDays(-14)),
                new(212, FebruaryDay.AddDays(-3)),
                new(213, FebruaryDay.AddDays(-2)),
                new(214, FebruaryDay.AddDays(-1)),
                new(215, FebruaryDay)
            };

            var barDaysPreviousMonth = new WikiPageStats.DayStat[]
            {
                new(101, FebruaryDay.AddMonths(-1).AddDays(-14)),
                new(102, FebruaryDay.AddMonths(-1).AddDays(-13)),
                new(115, FebruaryDay.AddMonths(-1)),
                new(131, FebruaryDay.AddDays(-15))
            };

            var barDaysCurrentMonth = new WikiPageStats.DayStat[]
            {
                new(201, FebruaryDay.AddDays(-14)),
                new(215, FebruaryDay),
                new(216, FebruaryDay.AddDays(1)),
                new(217, FebruaryDay.AddDays(2)),
                new(228, FebruaryDay.AddDays(13))
            };

            return new ValidWikiPagesStatsTestData(FebruaryDay,
                fooDaysPreviousMonth,
                fooDaysCurrentMonth,
                barDaysPreviousMonth,
                barDaysCurrentMonth);
        }
    }
}