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

        // kja this test will be unnecessary once I put constraints on the input
        //(see todos in Wikitools.AzureDevOps.AdoApi.GetAllWikiPagesDetails)
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
                new WikiPageStats.DayStat[] { }, // kja this test fails because now this is (12300, "/Bar"), causing one ID twice in the inputs...
                // ... note that currently the ID overrides duplicate IDs per each month, breaking invariants. The same ID is in all four quadrants.
                // What I actually want is to have (12300, Foo) in previous month (as Foo in the fixture) and (42000, Bar) in previous month
                // and (12300, Bar) in the current month (as Bar fixture). Thus the params of FooPageId and BarPageId should work differently:
                // the override should allow to instead say "in current month page 12300 was renamed to Bar; page 42000 no longer exists.
                // KJA NEXT TASK TO DO. Once test all tests green, do the type invariants, then fix the UnionUsing and add tests for it.
                // Probably I need params like that instead:
                // FooPagePathInCurrentMonth: "different name" // null denotes "no longer exists"
                // BarPagePathInCurrentMonth: same as above.
                //
                // kja I will probably also want another test: that not only renames (12300, Foo) to (12300, Bar), but at the same time
                // renames (42000, Bar) to (42000, Foo), and things still work out.
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
                // kja this test also violates type invariant from AdoApi (see todos there), as it should not be 
                // possible to have two IDs with the same path in the same month, and this currently does that.
                //
                // Ultimately this test will be redundant with the test above.
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