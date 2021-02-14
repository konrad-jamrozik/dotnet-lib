using System;
using System.Linq;
using MoreLinq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Tests
{
    // kja NEXT: add Merge tests.
    // Some tests:
    // DONE Split(x,x) = x, where x is only for one month
    // DONE Merge(Split(x,y)) == (x,y)
    // DONE Split(Merge(x,y)) == (x,y)
    // DONE Merge(x,x) == x
    // DONE Merge(page1, page2) == stats for both page1 and page2
    // Merge(page1month1, page1month2) == stats for both months for page1
    // Merge(page1day1, page1day2) == stats for both days for page1
    // DONE Merge(page1day1, page1day1) == either one stat, or error if different counts
    // Some test checking which page rename takes precedence, both in correct ((prev,curr)) and flipped ((curr,prev)) ordering.
    //   Do this test by passing the same data but flipped current/previous.
    public class WikiPagesStatsStorageTests
    {
        // kja convert to theory with data class
        [Fact] public void ParameterizedTest() => Verify();

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

        private static TestPayload PageStatsBeforeYearWrap =>
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
                },
                SplitByMonthThrows: true);

        private static TestPayload PageStatsSamePreviousDay =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) },
                MergedDayStats: new[]
                { 
                    new WikiPageStats.DayStat[] {}, 
                    new WikiPageStats.DayStat[] { new(103, JanuaryDate)}
                },
                SplitByMonthThrows: true);

        private static TestPayload PageStatsSameDayDifferentCounts =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { },
                new WikiPageStats.DayStat[] { new(215, FebruaryDate) },
                new WikiPageStats.DayStat[] { new(217, FebruaryDate) },
                SplitByMonthThrows: true,
                MergeThrows: true);

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

        private record TestPayload(
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

        private static void Verify()
        {
            TestPayload[] payloads =
            {
                PageStatsEmpty,
                PageStatsPreviousMonthOnly,
                PageStatsYearWrap,
                PageStatsBeforeYearWrap,
                PageStats,
                PageStatsSameDay,
                // kja curr work
                PageStatsSamePreviousDay,
                PageStatsSameDayDifferentCounts
            };
            payloads.ForEach(Verify);
        }

        private static void Verify(TestPayload data)
        {
            // Act - Split({foo, bar})
            (WikiPageStats[] previousMonth, WikiPageStats[] currentMonth)? split = !data.SplitByMonthThrows 
                ? VerifySplitByMonth(data) : VerifySplitByMonthThrows(data);

            // Act - Merge(foo, bar)
            WikiPageStats[]? merged = !data.MergeThrows ? VerifyMerge(data) : VerifyMergeThrows(data);

            if (data.MergeThrows || data.SplitByMonthThrows) 
                return;

            // Act - Split(Merge(foo, bar)) == (foo, bar)
            var (previousMonthUnmerged, currentMonthUnmerged) =
                WikiPagesStatsStorage.SplitByMonth(merged!, data.Date);
            new JsonDiffAssertion(data.PreviousMonth, previousMonthUnmerged).Assert();
            new JsonDiffAssertion(data.CurrentMonth,  currentMonthUnmerged).Assert();

            // Act - Merge(Split({foo, bar})) == Merge(foo, bar)
            var mergedSplit = WikiPagesStatsStorage.Merge(split!.Value.previousMonth, split!.Value.currentMonth);
            new JsonDiffAssertion(data.MergedPagesStats, mergedSplit).Assert();
        }

        private static (WikiPageStats[] previousMonth, WikiPageStats[] currentMonth) VerifySplitByMonth(
            TestPayload data)
        {
            // Act
            var (previousMonth, currentMonth) = WikiPagesStatsStorage.SplitByMonth(data.AllPagesStats, data.Date);

            new JsonDiffAssertion(data.PreviousMonth, previousMonth).Assert();
            new JsonDiffAssertion(data.CurrentMonth,  currentMonth).Assert();

            return (previousMonth, currentMonth);
        }

        private static WikiPageStats[] VerifyMerge(TestPayload data)
        {
            // Act
            var merged = WikiPagesStatsStorage.Merge(data.PreviousMonth, data.CurrentMonth);

            new JsonDiffAssertion(data.MergedPagesStats, merged).Assert();

            return merged;
        }

        private static (WikiPageStats[] previousMonth, WikiPageStats[] currentMonth)? VerifySplitByMonthThrows(TestPayload data)
        {
            try
            {
                VerifySplitByMonth(data);
            }
            catch (ArgumentException)
            {
                // Pass
                return null;
            }

            Assert.False(true);
            return null;
        }

        private static WikiPageStats[]? VerifyMergeThrows(TestPayload data)
        {
            try
            {
                VerifyMerge(data);
            }
            catch (ArgumentException)
            {
                // Pass
                return null;
            }

            Assert.False(true);
            return null;
        }
    }
}