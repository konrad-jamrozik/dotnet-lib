using System;
using Wikitools.AzureDevOps;

namespace Wikitools.Tests
{
    public static class WikiPagesStatsStorageTestsData
    {
        // @formatter:off
        private static readonly DateTime  JanuaryDate = new DateTime(year: 2021, month:  1, day:  3).ToUniversalTime();
        private static readonly DateTime FebruaryDate = new DateTime(year: 2021, month:  2, day: 15).ToUniversalTime();
        private static readonly DateTime DecemberDate = new DateTime(year: 2020, month: 12, day: 22).ToUniversalTime();
        // @formatter:on

        public static WikiPagesStatsTestData PageStatsEmpty =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { });

        public static WikiPagesStatsTestData PageStatsPreviousMonthOnly =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(115, FebruaryDate.AddMonths(-1)) },
                new WikiPageStats.DayStat[] { });

        public static WikiPagesStatsTestData PageStatsYearWrap =>
            new(JanuaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(1201, JanuaryDate.AddMonths(-1).AddDays(-2)) },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) });

        public static WikiPagesStatsTestData PageStatsBeforeYearWrap =>
            new(DecemberDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(1122, DecemberDate.AddMonths(-1)) },
                new WikiPageStats.DayStat[] { new(1223, DecemberDate.AddDays(1)) });

        public static WikiPagesStatsTestData PageStatsSameDay =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(215, FebruaryDate) },
                new WikiPageStats.DayStat[] { new(215, FebruaryDate) },
                MergedDayStats:
                (
                    new WikiPageStats.DayStat[] {}, 
                    new WikiPageStats.DayStat[] { new(215, FebruaryDate)}
                ),
                SplitByMonthThrows: true);

        public static WikiPagesStatsTestData PageStatsSamePreviousDay =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                MergedDayStats:
                ( 
                    new WikiPageStats.DayStat[] {}, 
                    new WikiPageStats.DayStat[] { new(103, JanuaryDate)}
                ),
                SplitByMonthThrows: true);

        public static WikiPagesStatsTestData PageStatsSameDayDifferentCounts =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(215, FebruaryDate) },
                new WikiPageStats.DayStat[] { new(217, FebruaryDate) },
                SplitByMonthThrows: true,
                MergeThrows: true);

        public static WikiPagesStatsTestData PageStatsUnorderedDayStats =>
            new(FebruaryDate,
                FooPagePreviousDayStats: new WikiPageStats.DayStat[]
                {
                    new(107, JanuaryDate.AddDays(4)), 
                    new(217, FebruaryDate.AddDays(2)), 
                    new(103, JanuaryDate)
                },
                FooPageCurrentDayStats: new WikiPageStats.DayStat[]
                {
                    new(219, FebruaryDate.AddDays(4)), 
                    new(105, JanuaryDate.AddDays(2)), 
                    new(215, FebruaryDate)
                },
                new WikiPageStats.DayStat[] {},
                new WikiPageStats.DayStat[] {},
                FooPagePreviousMonthDayStats: new WikiPageStats.DayStat[]
                {
                    new(103, JanuaryDate),
                    new(105, JanuaryDate.AddDays(2)),
                    new(107, JanuaryDate.AddDays(4)), 
                },
                FooPageCurrentMonthDayStats: new WikiPageStats.DayStat[]
                {
                    new(215, FebruaryDate),
                    new(217, FebruaryDate.AddDays(2)),
                    new(219, FebruaryDate.AddDays(4)), 
                },
                MergedDayStats:
                ( 
                    new WikiPageStats.DayStat[]
                    {
                        new(103, JanuaryDate),
                        new(105, JanuaryDate.AddDays(2)), 
                        new(107, JanuaryDate.AddDays(4)), 
                        new(215, FebruaryDate),
                        new(217, FebruaryDate.AddDays(2)),
                        new(219, FebruaryDate.AddDays(4))
                    }, 
                    new WikiPageStats.DayStat[] { }
                ));

        public static WikiPagesStatsTestData PageStatsRenamedToNewPath =>
            new(FebruaryDate,
                FooPagePreviousDayStats: new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                BarPageCurrentDayStats: new WikiPageStats.DayStat[] { new(217, FebruaryDate.AddDays(2)) },
                FooPageId: 12300,
                BarPageId: 12300 // Page /Foo was renamed to /Bar, thus the same ID
            );

        public static WikiPagesStatsTestData PageStatsRenamedToExistingPath =>
            new(FebruaryDate,
                FooPagePreviousDayStats: new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                BarPageCurrentDayStats: new WikiPageStats.DayStat[] { new(217, FebruaryDate.AddDays(2)) },
                FooPagePath: "/Foo",
                BarPagePath: "/Foo" // Bar was renamed to "/Foo". Assuming here that old Foo no longer exists.
            );

        public static WikiPagesStatsTestData PageStats
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

                return new WikiPagesStatsTestData(FebruaryDate,
                    fooDaysPreviousMonth,
                    fooDaysCurrentMonth,
                    barDaysPreviousMonth,
                    barDaysCurrentMonth);
            }
        }

        public static WikiPagesStatsTestData PageStatsSameMonth =>
            new(FebruaryDate,
                FooPagePreviousDayStats: new WikiPageStats.DayStat[]
                {
                    new(108, JanuaryDate.AddDays(5)),
                    new(218, FebruaryDate.AddDays(3))
                },
                FooPageCurrentDayStats: new WikiPageStats.DayStat[]
                {
                    new(104, JanuaryDate.AddDays(1)),
                    new(108, JanuaryDate.AddDays(5)),
                    new(110, JanuaryDate.AddDays(7))
                },
                BarPagePreviousDayStats: new WikiPageStats.DayStat[]
                {
                    new(215, FebruaryDate), 
                    new(216, FebruaryDate.AddDays(1)),
                    new(103, JanuaryDate)
                },
                BarPageCurrentDayStats: new WikiPageStats.DayStat[]
                {
                    new(216, FebruaryDate.AddDays(1)), 
                    new(217, FebruaryDate.AddDays(2))
                },
                MergedDayStats:
                ( 
                    new WikiPageStats.DayStat[]
                    {
                        new(104, JanuaryDate.AddDays(1)),
                        new(108, JanuaryDate.AddDays(5)),
                        new(110, JanuaryDate.AddDays(7)),
                        new(218, FebruaryDate.AddDays(3)),
                        
                    }, 
                    new WikiPageStats.DayStat[]
                    {
                        new(103, JanuaryDate),
                        new(215, FebruaryDate), 
                        new(216, FebruaryDate.AddDays(1)), 
                        new(217, FebruaryDate.AddDays(2))
                    }
                ),
                SplitByMonthThrows: true);
    }
}