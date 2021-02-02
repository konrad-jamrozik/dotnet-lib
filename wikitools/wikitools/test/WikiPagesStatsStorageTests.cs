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
            var (pageStats, expectedLastMonth, expectedThisMonth) = PageStatsData;

            // Act
            var (lastMonth, thisMonth) = WikiPagesStatsStorage.SplitByMonth(pageStats);

            new JsonDiffAssertion(expectedLastMonth, lastMonth).Assert();
            new JsonDiffAssertion(expectedThisMonth, thisMonth).Assert();
        }

        private static (WikiPageStats[] pageStats, WikiPageStats[] expectedLastMonth, WikiPageStats[] expectedThisMonth)
            PageStatsData
        {
            get
            {
                var date = new DateTime(year: 2021, month: 2, day: 15).ToUniversalTime();

                var fooDaysLastMonth = new WikiPageStats.Stat[]
                {
                    new(113, date.AddMonths(-1).AddDays(-2)),
                    new(114, date.AddMonths(-1).AddDays(-1)),
                    new(115, date.AddMonths(-1)),
                    new(131, date.AddDays(-15))
                };

                var fooDaysThisMonth = new WikiPageStats.Stat[]
                {
                    new(201, date.AddDays(-14)),
                    new(212, date.AddDays(-3)),
                    new(213, date.AddDays(-2)),
                    new(214, date.AddDays(-1)),
                    new(215, date)
                };

                var barDaysLastMonth = new WikiPageStats.Stat[]
                {
                    new(101, date.AddMonths(-1).AddDays(-14)),
                    new(102, date.AddMonths(-1).AddDays(-13)),
                    new(115, date.AddMonths(-1)),
                    new(131, date.AddDays(-15))
                };

                var barDaysThisMonth = new WikiPageStats.Stat[]
                {
                    new(201, date.AddDays(-14)),
                    new(215, date),
                    new(216, date.AddDays(1)),
                    new(217, date.AddDays(2)),
                    new(228, date.AddDays(13))
                };

                var pageStats = new WikiPageStats[]
                {
                    new("/Foo", 1, fooDaysLastMonth.Concat(fooDaysThisMonth).ToArray()),
                    new("/Bar", 2, barDaysLastMonth.Concat(barDaysThisMonth).ToArray())
                };

                var expectedLastMonth = new WikiPageStats[]
                {
                    new("/Foo", 1, fooDaysLastMonth),
                    new("/Bar", 2, barDaysLastMonth)
                };

                var expectedThisMonth = new WikiPageStats[]
                {
                    new("/Foo", 1, fooDaysThisMonth),
                    new("/Bar", 2, barDaysThisMonth)
                };

                return (pageStats, expectedLastMonth, expectedThisMonth);
            }
        }
    }
}