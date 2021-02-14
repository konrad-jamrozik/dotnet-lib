using System;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Tests
{
    // kja NEXT: add Merge tests.
    // Some tests:
    // Split(x,x) = x, where x is only for one month
    // Merge(Split(x,y)) == (x,y)
    // Split(Merge(x,y)) == (x,y)
    // Merge(x,x) == x
    // Merge(page1, page2) == stats for both page1 and page2
    // Merge(page1month1, page1month2) == stats for both months for page1
    // Merge(page1day1, page1day2) == stats for both days for page1
    // Some test checking which page rename takes precedence, both in correct ((prev,curr)) and flipped ((curr,prev)) ordering.
    public class WikiPagesStatsStorageTests
    {
        // @formatter:off
        [Fact] public void SplitByMonthTestNoStats()            => VerifySplitByMonth(PageStatsEmpty);
        [Fact] public void SplitByMonthTestOnlyPreviousMonth()  => VerifySplitByMonth(PageStatsPreviousMonthOnly);
        [Fact] public void SplitByMonthTestYearWrap()           => VerifySplitByMonth(PageStatsYearWrap);
        [Fact] public void SplitByMonthTestJustBeforeYearWrap() => VerifySplitByMonth(PageStatsJustBeforeYearWrap);
        [Fact] public void SplitByMonthTest()                   => VerifySplitByMonth(PageStats);
        // @formatter:on

        private static readonly DateTime JanuaryDate = new DateTime(year: 2021,  month: 1,  day: 3).ToUniversalTime();
        private static readonly DateTime FebruaryDate = new DateTime(year: 2021, month: 2,  day: 15).ToUniversalTime();
        private static readonly DateTime DecemberDate = new DateTime(year: 2021, month: 12, day: 22).ToUniversalTime();

        private static TestPayload PageStatsEmpty =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[0],
                new WikiPageStats.DayStat[0],
                new WikiPageStats.DayStat[0],
                new WikiPageStats.DayStat[0]);

        private static TestPayload PageStatsPreviousMonthOnly =>
            new(FebruaryDate,
                new WikiPageStats.DayStat[0],
                new WikiPageStats.DayStat[0],
                new WikiPageStats.DayStat[] { new(115, FebruaryDate.AddMonths(-1)) },
                new WikiPageStats.DayStat[0]);

        private static TestPayload PageStatsYearWrap =>
            new(JanuaryDate,
                new WikiPageStats.DayStat[0],
                new WikiPageStats.DayStat[0],
                new WikiPageStats.DayStat[] { new(1201, JanuaryDate.AddMonths(-1).AddDays(-2)) },
                new WikiPageStats.DayStat[] { new(103, JanuaryDate) });

        private static TestPayload PageStatsJustBeforeYearWrap =>
            new(DecemberDate,
                new WikiPageStats.DayStat[0],
                new WikiPageStats.DayStat[0],
                new WikiPageStats.DayStat[] { new(1122, DecemberDate.AddMonths(-1)) },
                new WikiPageStats.DayStat[] { new(1223, DecemberDate.AddDays(1)) });

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
            WikiPageStats.DayStat[] BarPageCurrentDays)
        {
            private WikiPageStats FooPage => new("/Foo", 100, FooPagePreviousDays.Concat(FooPageCurrentDays).ToArray());
            private WikiPageStats BarPage => new("/Bar", 200, BarPagePreviousDays.Concat(BarPageCurrentDays).ToArray());

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

            public WikiPageStats[] PageStats => new[] { FooPage, BarPage };
        }

        private static void VerifySplitByMonth(TestPayload data)
        {
            // Act
            var (previousMonth, currentMonth) = WikiPagesStatsStorage.SplitByMonth(data.PageStats, data.Date);

            new JsonDiffAssertion(data.PreviousMonth, previousMonth).Assert();
            new JsonDiffAssertion(data.CurrentMonth,  currentMonth).Assert();
        }
    }
}