using System;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps.Tests
{
    public static class ValidWikiPagesStatsTestDataFixture
    {
        // @formatter:off
        private static readonly DateTime DecemberDate = new DateTime(year: 2020, month: 12, day: 22).Utc();
        private static readonly DateTime  JanuaryDate = new DateTime(year: 2021, month:  1, day:  3).Utc();
        private static readonly DateTime FebruaryDate = new DateTime(year: 2021, month:  2, day: 15).Utc();
        // @formatter:on

        public static ValidWikiPagesStatsTestData PageStatsEmpty =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { });

        public static ValidWikiPagesStatsTestData PageStatsPreviousMonthOnly =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(115, FebruaryDate.AddMonths(-1)) },
                new WikiPageStats.DayStat[] { });

        public static ValidWikiPagesStatsTestData PageStatsYearWrap =>
            new(JanuaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(1201, JanuaryDate.AddMonths(-1).AddDays(-2)) },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) });

        public static ValidWikiPagesStatsTestData PageStatsBeforeYearWrap =>
            new(DecemberDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(1122, DecemberDate.AddMonths(-1)) },
                new WikiPageStats.DayStat[] { new(1223, DecemberDate.AddDays(1)) });

        public static ValidWikiPagesStatsTestData PageStatsRenamedToNewPath =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                new WikiPageStats.DayStat[] { new(217, FebruaryDate.AddDays(2)) },
                new WikiPageStats.DayStat[] { new(104, JanuaryDate.AddDays(1)) },
                new WikiPageStats.DayStat[] { new(219, FebruaryDate.AddDays(4)) },
                FooPagePathInCurrentMonth: "/Qux"
            );

        public static ValidWikiPagesStatsTestData PageStatsExchangedPaths =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                new WikiPageStats.DayStat[] { new(217, FebruaryDate.AddDays(2)) },
                new WikiPageStats.DayStat[] { new(104, JanuaryDate.AddDays(1)) },
                new WikiPageStats.DayStat[] { new(219, FebruaryDate.AddDays(4)) },
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
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(219, FebruaryDate.AddDays(4)) },
                FooPageDeletedInCurrentMonth: true,
                BarPageDeletedInPreviousMonth: true
            );

        public static ValidWikiPagesStatsTestData PageStats
        {
            get
            {
                var fooDaysPreviousMonth = new WikiPageStats.DayStat[]
                {
                    new(113, FebruaryDate.AddMonths(-1).AddDays(-2)),
                    new(114, FebruaryDate.AddMonths(-1).AddDays(-1)),
                    new(115, FebruaryDate.AddMonths(-1)),
                    new(131, FebruaryDate.AddDays(-15))
                };

                var fooDaysCurrentMonth = new WikiPageStats.DayStat[]
                {
                    new(201, FebruaryDate.AddDays(-14)),
                    new(212, FebruaryDate.AddDays(-3)),
                    new(213, FebruaryDate.AddDays(-2)),
                    new(214, FebruaryDate.AddDays(-1)),
                    new(215, FebruaryDate)
                };

                var barDaysPreviousMonth = new WikiPageStats.DayStat[]
                {
                    new(101, FebruaryDate.AddMonths(-1).AddDays(-14)),
                    new(102, FebruaryDate.AddMonths(-1).AddDays(-13)),
                    new(115, FebruaryDate.AddMonths(-1)),
                    new(131, FebruaryDate.AddDays(-15))
                };

                var barDaysCurrentMonth = new WikiPageStats.DayStat[]
                {
                    new(201, FebruaryDate.AddDays(-14)),
                    new(215, FebruaryDate),
                    new(216, FebruaryDate.AddDays(1)),
                    new(217, FebruaryDate.AddDays(2)),
                    new(228, FebruaryDate.AddDays(13))
                };

                return new ValidWikiPagesStatsTestData(FebruaryDate,
                    fooDaysPreviousMonth,
                    fooDaysCurrentMonth,
                    barDaysPreviousMonth,
                    barDaysCurrentMonth);
            }
        }
    }
}