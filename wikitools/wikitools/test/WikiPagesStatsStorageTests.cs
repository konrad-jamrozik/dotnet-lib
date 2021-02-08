using System;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Tests
{
    public class WikiPagesStatsStorageTests
    {
        private static readonly DateTime JanuaryDate = new DateTime(year: 2021,  month: 1,  day: 3).ToUniversalTime();
        private static readonly DateTime FebruaryDate = new DateTime(year: 2021, month: 2,  day: 15).ToUniversalTime();
        private static readonly DateTime DecemberDate = new DateTime(year: 2021, month: 12, day: 22).ToUniversalTime();

        [Fact]
        public void SplitByMonthTest() => Verify(PageStatsData);

        [Fact]
        public void SplitByMonthTestNoStats() => Verify(BuildTestPayload(FebruaryDate));

        [Fact]
        public void SplitByMonthTestOnlyPreviousMonth() => Verify(PageStatsDataPreviousMonthOnly);

        [Fact]
        public void SplitByMonthTestYearWrap() => Verify(PageStatsDataYearWrap);

        [Fact]
        public void SplitByMonthTestJustBeforeYearWrap() => Verify(PageStatsDataJustBeforeYearWrap);

        private static void Verify(TestPayload data)
        {
            // Act
            var (previousMonth, currentMonth) = WikiPagesStatsStorage.SplitByMonth(data.PageStats, data.Date);

            new JsonDiffAssertion(data.ExpectedPreviousMonth, previousMonth).Assert();
            new JsonDiffAssertion(data.ExpectedCurrentMonth,  currentMonth).Assert();
        }

        private static TestPayload PageStatsData
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

                return BuildTestPayload(
                    FebruaryDate,
                    fooDaysPreviousMonth,
                    fooDaysCurrentMonth,
                    barDaysPreviousMonth,
                    barDaysCurrentMonth);
            }
        }

        private static TestPayload PageStatsDataPreviousMonthOnly =>
            BuildTestPayload(FebruaryDate,
                barDaysPreviousMonth: new WikiPageStats.DayStat[]
                {
                    new(115, FebruaryDate.AddMonths(-1))
                });

        private static TestPayload PageStatsDataYearWrap =>
            BuildTestPayload(JanuaryDate,
                barDaysPreviousMonth: new WikiPageStats.DayStat[]
                {
                    new(1201, JanuaryDate.AddMonths(-1).AddDays(-2))
                },
                barDaysCurrentMonth: new WikiPageStats.DayStat[]
                {
                    new(103, JanuaryDate)
                });

        private static TestPayload PageStatsDataJustBeforeYearWrap =>
            BuildTestPayload(DecemberDate,
                barDaysPreviousMonth: new WikiPageStats.DayStat[]
                {
                    new(1122, DecemberDate.AddMonths(-1))
                },
                barDaysCurrentMonth: new WikiPageStats.DayStat[]
                {
                    new(1223, DecemberDate.AddDays(1))
                });

        private static TestPayload
            BuildTestPayload(
                DateTime date,
                WikiPageStats.DayStat[]? fooDaysPreviousMonth = null,
                WikiPageStats.DayStat[]? fooDaysCurrentMonth = null,
                WikiPageStats.DayStat[]? barDaysPreviousMonth = null,
                WikiPageStats.DayStat[]? barDaysCurrentMonth = null)
        {
            fooDaysPreviousMonth ??= Array.Empty<WikiPageStats.DayStat>();
            fooDaysCurrentMonth ??= Array.Empty<WikiPageStats.DayStat>();
            barDaysPreviousMonth ??= Array.Empty<WikiPageStats.DayStat>();
            barDaysCurrentMonth ??= Array.Empty<WikiPageStats.DayStat>();

            var pageStats = new WikiPageStats[]
            {
                new("/Foo", 100, fooDaysPreviousMonth.Concat(fooDaysCurrentMonth).ToArray()),
                new("/Bar", 200, barDaysPreviousMonth.Concat(barDaysCurrentMonth).ToArray())
            };

            var expectedPreviousMonth = new WikiPageStats[]
            {
                new("/Foo", 100, fooDaysPreviousMonth),
                new("/Bar", 200, barDaysPreviousMonth)
            };

            var expectedCurrentMonth = new WikiPageStats[]
            {
                new("/Foo", 100, fooDaysCurrentMonth),
                new("/Bar", 200, barDaysCurrentMonth)
            };

            return new TestPayload(date, pageStats, expectedPreviousMonth, expectedCurrentMonth);
        }

        private record TestPayload(
            DateTime Date,
            WikiPageStats[] PageStats,
            WikiPageStats[] ExpectedPreviousMonth,
            WikiPageStats[] ExpectedCurrentMonth);
    }
}