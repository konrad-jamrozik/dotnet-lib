using System;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Tests
{
    public class WikiPagesStatsStorageTests
    {
        [Fact]
        public void SplitsByMonth()
        {
            var (pageStats, expectedPreviousMonth, expectedCurrentMonth) = PageStatsData;

            // Act
            var (previousMonth, currentMonth) = WikiPagesStatsStorage.SplitByMonth(pageStats);

            new JsonDiffAssertion(expectedPreviousMonth, previousMonth).Assert();
            new JsonDiffAssertion(expectedCurrentMonth, currentMonth).Assert();
        }

        private static (WikiPageStats[] pageStats, WikiPageStats[] expectedPreviousMonth, WikiPageStats[] expectedCurrentMonth)
            PageStatsData
        {
            get
            {
                var date = new DateTime(year: 2021, month: 2, day: 15).ToUniversalTime();

                var fooDaysPreviousMonth = new WikiPageStats.Stat[]
                {
                    new(113, date.AddMonths(-1).AddDays(-2)),
                    new(114, date.AddMonths(-1).AddDays(-1)),
                    new(115, date.AddMonths(-1)),
                    new(131, date.AddDays(-15))
                };

                var fooDaysCurrentMonth = new WikiPageStats.Stat[]
                {
                    new(201, date.AddDays(-14)),
                    new(212, date.AddDays(-3)),
                    new(213, date.AddDays(-2)),
                    new(214, date.AddDays(-1)),
                    new(215, date)
                };

                var barDaysPreviousMonth = new WikiPageStats.Stat[]
                {
                    new(101, date.AddMonths(-1).AddDays(-14)),
                    new(102, date.AddMonths(-1).AddDays(-13)),
                    new(115, date.AddMonths(-1)),
                    new(131, date.AddDays(-15))
                };

                var barDaysCurrentMonth = new WikiPageStats.Stat[]
                {
                    new(201, date.AddDays(-14)),
                    new(215, date),
                    new(216, date.AddDays(1)),
                    new(217, date.AddDays(2)),
                    new(228, date.AddDays(13))
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
    }
}