using System;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Tests
{
    // kja NEXT: add Merge tests.
    // Some tests:
    // DONE Split(x,x) = x, where x is only for one month
    // Merge(Split(x,y)) == (x,y)
    // Split(Merge(x,y)) == (x,y)
    // Merge(x,x) == x
    // DONE Merge(page1, page2) == stats for both page1 and page2
    // Merge(page1month1, page1month2) == stats for both months for page1
    // Merge(page1day1, page1day2) == stats for both days for page1
    // DONE Merge(page1day1, page1day1) == either one stat, or error if different counts
    // Some test checking which page rename takes precedence, both in correct ((prev,curr)) and flipped ((curr,prev)) ordering.
    public class WikiPagesStatsStorageTests
    {
        // @formatter:off
        [Fact] public void SplitByMonthTestNoStats()            => VerifySplitByMonth(PageStatsEmpty);
        [Fact] public void SplitByMonthTestOnlyPreviousMonth()  => VerifySplitByMonth(PageStatsPreviousMonthOnly);
        [Fact] public void SplitByMonthTestYearWrap()           => VerifySplitByMonth(PageStatsYearWrap);
        [Fact] public void SplitByMonthTestJustBeforeYearWrap() => VerifySplitByMonth(PageStatsJustBeforeYearWrap);
        [Fact] public void SplitByMonthTest()                   => VerifySplitByMonth(PageStats);
        [Fact] public void SplitByMonthTestSameDay()            => VerifySplitByMonthThrows(PageStatsSameDay);
        [Fact] public void SplitByMonthTestSamePreviousDay()    => VerifySplitByMonthThrows(PageStatsSamePreviousDay);
        // @formatter:on

        // @formatter:off
        [Fact] public void MergeTestNoStats()           => VerifyMerge(PageStatsEmpty);
        [Fact] public void MergeTestPreviousMonthOnly() => VerifyMerge(PageStatsPreviousMonthOnly);
        [Fact] public void MergeTest()                  => VerifyMerge(PageStats);
        [Fact] public void MergeTestSameDay()           => VerifyMerge(PageStatsSameDay);
        // @formatter:on

        // @formatter:off
        [Fact] public void MergeTestSameDayDifferentCounts() => VerifyMergeThrows(PageStatsSameDayDifferentCounts);
        // @formatter:on

        // @formatter:off
        private static readonly DateTime  JanuaryDate = new DateTime(year: 2021, month:  1, day:  3).ToUniversalTime();
        private static readonly DateTime FebruaryDate = new DateTime(year: 2021, month:  2, day: 15).ToUniversalTime();
        private static readonly DateTime DecemberDate = new DateTime(year: 2020, month: 12, day: 22).ToUniversalTime();
        // @formatter:on

        private static TestPayload PageStatsEmpty =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { });

        private static TestPayload PageStatsPreviousMonthOnly =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(115, FebruaryDate.AddMonths(-1)) },
                new WikiPageStats.DayStat[] { });

        private static TestPayload PageStatsYearWrap =>
            new(JanuaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(1201, JanuaryDate.AddMonths(-1).AddDays(-2)) },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) });

        private static TestPayload PageStatsJustBeforeYearWrap =>
            new(DecemberDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(1122, DecemberDate.AddMonths(-1)) },
                new WikiPageStats.DayStat[] { new(1223, DecemberDate.AddDays(1)) });

        private static TestPayload PageStatsSameDay =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(215, FebruaryDate) },
                new WikiPageStats.DayStat[] { new(215, FebruaryDate) },
                MergedDayStats: new[]
                { 
                    new WikiPageStats.DayStat[] {}, 
                    new WikiPageStats.DayStat[] { new(215, FebruaryDate)}
                });

        private static TestPayload PageStatsSameDayDifferentCounts =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(215, FebruaryDate) },
                new WikiPageStats.DayStat[] { new(217, FebruaryDate) });

        private static TestPayload PageStatsSamePreviousDay =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) });

        

        private static TestPayload PageStats
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

                return new TestPayload(FebruaryDate,
                    fooDaysPreviousMonth,
                    fooDaysCurrentMonth,
                    barDaysPreviousMonth,
                    barDaysCurrentMonth);
            }
        }


        // kja pass as input: expected splitByMonth and MergeBehaviors: green/throw.
        private record TestPayload(
            DateTime Date,
            WikiPageStats.DayStat[] FooPagePreviousDays,
            WikiPageStats.DayStat[] FooPageCurrentDays,
            WikiPageStats.DayStat[] BarPagePreviousDays,
            WikiPageStats.DayStat[] BarPageCurrentDays,
            WikiPageStats.DayStat[][]? MergedDayStats = null,
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

        private static void VerifySplitByMonth(TestPayload data)
        {
            // Act
            var (previousMonth, currentMonth) = WikiPagesStatsStorage.SplitByMonth(data.AllPagesStats, data.Date);

            new JsonDiffAssertion(data.PreviousMonth, previousMonth).Assert();
            new JsonDiffAssertion(data.CurrentMonth,  currentMonth).Assert();
        }

        private static void VerifyMerge(TestPayload data)
        {
            // Act
            var merged = WikiPagesStatsStorage.Merge(data.PreviousMonth, data.CurrentMonth);

            new JsonDiffAssertion(data.MergedPagesStats, merged).Assert();
        }

        private static void VerifySplitByMonthThrows(TestPayload data)
        {
            try
            {
                VerifySplitByMonth(data);
            }
            catch (ArgumentException)
            {
                // Pass
                return;
            }

            Assert.False(true);
        }

        private static void VerifyMergeThrows(TestPayload data)
        {
            try
            {
                VerifyMerge(data);
            }
            catch (ArgumentException)
            {
                // Pass
                return;
            }

            Assert.False(true);
        }
    }
}