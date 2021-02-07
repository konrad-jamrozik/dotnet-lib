using System;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Tests
{
    public class WikiPagesStatsStorageTests
    {
        private static readonly DateTime CurrentDate = new DateTime(year: 2021, month: 2, day: 15).ToUniversalTime();

        [Fact]
        public void SplitByMonthTest()
        {
            var (pageStats, expectedPreviousMonth, expectedCurrentMonth) = PageStatsData;

            // Act
            var (previousMonth, currentMonth) = WikiPagesStatsStorage.SplitByMonth(pageStats, CurrentDate);

            new JsonDiffAssertion(expectedPreviousMonth, previousMonth).Assert();
            new JsonDiffAssertion(expectedCurrentMonth,  currentMonth).Assert();
        }

        [Fact]
        public void SplitByMonthTestOnlyPreviousMonth()
        {
            var (pageStats, expectedPreviousMonth, expectedCurrentMonth) = PageStatsDataPreviousMonthOnly;

            // Act
            var (previousMonth, currentMonth) = WikiPagesStatsStorage.SplitByMonth(pageStats, CurrentDate);

            new JsonDiffAssertion(expectedPreviousMonth, previousMonth).Assert();
            new JsonDiffAssertion(expectedCurrentMonth,  currentMonth).Assert();
        }


        private static (WikiPageStats[] pageStats, WikiPageStats[] expectedPreviousMonth, WikiPageStats[]
            expectedCurrentMonth)
            PageStatsData
        {
            get
            {
                var fooDaysPreviousMonth = new WikiPageStats.Stat[]
                {
                    new(113, CurrentDate.AddMonths(-1).AddDays(-2)),
                    new(114, CurrentDate.AddMonths(-1).AddDays(-1)),
                    new(115, CurrentDate.AddMonths(-1)),
                    new(131, CurrentDate.AddDays(-15))
                };

                var fooDaysCurrentMonth = new WikiPageStats.Stat[]
                {
                    new(201, CurrentDate.AddDays(-14)),
                    new(212, CurrentDate.AddDays(-3)),
                    new(213, CurrentDate.AddDays(-2)),
                    new(214, CurrentDate.AddDays(-1)),
                    new(215, CurrentDate)
                };

                var barDaysPreviousMonth = new WikiPageStats.Stat[]
                {
                    new(101, CurrentDate.AddMonths(-1).AddDays(-14)),
                    new(102, CurrentDate.AddMonths(-1).AddDays(-13)),
                    new(115, CurrentDate.AddMonths(-1)),
                    new(131, CurrentDate.AddDays(-15))
                };

                var barDaysCurrentMonth = new WikiPageStats.Stat[]
                {
                    new(201, CurrentDate.AddDays(-14)),
                    new(215, CurrentDate),
                    new(216, CurrentDate.AddDays(1)),
                    new(217, CurrentDate.AddDays(2)),
                    new(228, CurrentDate.AddDays(13))
                };

                var pageStats = new WikiPageStats[]
                {
                    new("/Foo", 1, fooDaysPreviousMonth.Concat(fooDaysCurrentMonth).ToArray()),
                    new("/Bar", 2, barDaysPreviousMonth.Concat(barDaysCurrentMonth).ToArray())
                };

                var expectedPreviousMonth = new WikiPageStats[]
                {
                    new("/Foo", 1, fooDaysPreviousMonth),
                    new("/Bar", 2, barDaysPreviousMonth)
                };

                var expectedCurrentMonth = new WikiPageStats[]
                {
                    new("/Foo", 1, fooDaysCurrentMonth),
                    new("/Bar", 2, barDaysCurrentMonth)
                };

                return (pageStats, expectedPreviousMonth, expectedCurrentMonth);
            }
        }

        private static (WikiPageStats[] pageStats, WikiPageStats[] expectedPreviousMonth, WikiPageStats[]
            expectedCurrentMonth)
            PageStatsDataPreviousMonthOnly
        {
            get
            {
                var fooDaysPreviousMonth = new WikiPageStats.Stat[] { };

                var fooDaysCurrentMonth = new WikiPageStats.Stat[] { };

                var barDaysPreviousMonth = new WikiPageStats.Stat[]
                {
                    new(115, CurrentDate.AddMonths(-1))
                };

                var barDaysCurrentMonth = new WikiPageStats.Stat[] { };

                var pageStats = new WikiPageStats[]
                {
                    new("/Foo", 1, fooDaysPreviousMonth.Concat(fooDaysCurrentMonth).ToArray()),
                    new("/Bar", 2, barDaysPreviousMonth.Concat(barDaysCurrentMonth).ToArray())
                };

                var expectedPreviousMonth = new WikiPageStats[]
                {
                    new("/Foo", 1, fooDaysPreviousMonth),
                    new("/Bar", 2, barDaysPreviousMonth)
                };

                var expectedCurrentMonth = new WikiPageStats[]
                {
                    new("/Foo", 1, fooDaysCurrentMonth),
                    new("/Bar", 2, barDaysCurrentMonth)
                };

                return (pageStats, expectedPreviousMonth, expectedCurrentMonth);
            }
        }

    }
}