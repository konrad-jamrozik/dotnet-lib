using System;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Xunit;
using Xunit.Sdk;

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

            var lastMonthDiff = new JsonDiff(expectedLastMonth, lastMonth);
            var thisMonthDiff = new JsonDiff(expectedThisMonth, thisMonth);
            Assert.True(lastMonthDiff.IsEmpty,
                $"lastMonthDiff: The expected baseline is different than actual target. Diff:{Environment.NewLine}{lastMonthDiff}");
            Assert.True(lastMonthDiff.IsEmpty,
                $"thisMonthDiff: The expected baseline is different than actual target. Diff:{Environment.NewLine}{thisMonthDiff}");
        }

        private static (WikiPageStats[] pageStats, WikiPageStats[] expectedLastMonth, WikiPageStats[] expectedThisMonth) PageStatsData
        {
            get
            {
                var date = new DateTime(year: 2021, month: 2, day: 15).ToUniversalTime();

                var pageStats = new WikiPageStats[]
                {
                    new("/Foo",
                        1,
                        new WikiPageStats.Stat[]
                        {
                            new(113, date.AddMonths(-1).AddDays(-2)),
                            new(114, date.AddMonths(-1).AddDays(-1)),
                            new(115, date.AddMonths(-1)),
                            new(131, date.AddDays(-15)),
                            new(201, date.AddDays(-14)),
                            new(212, date.AddDays(-3)),
                            new(213, date.AddDays(-2)),
                            new(214, date.AddDays(-1)),
                            new(215, date)
                        }),
                    new("/Bar",
                        2,
                        new WikiPageStats.Stat[]
                        {
                            new(101, date.AddMonths(-1).AddDays(-14)),
                            new(102, date.AddMonths(-1).AddDays(-13)),
                            new(115, date.AddMonths(-1)),
                            new(131, date.AddDays(-15)),
                            new(201, date.AddDays(-14)),
                            new(215, date),
                            new(216, date.AddDays(1)),
                            new(217, date.AddDays(2)),
                            new(228, date.AddDays(13))
                        })
                };

                var expectedLastMonth = new WikiPageStats[]
                {
                    new("/Foo",
                        1,
                        new WikiPageStats.Stat[]
                        {
                            new(113, date.AddMonths(-1).AddDays(-2)),
                            new(114, date.AddMonths(-1).AddDays(-1)),
                            new(115, date.AddMonths(-1)),
                            new(131, date.AddDays(-15)),
                        }),
                    new("/Bar",
                        2,
                        new WikiPageStats.Stat[]
                        {
                            new(101, date.AddMonths(-1).AddDays(-14)),
                            new(102, date.AddMonths(-1).AddDays(-13)),
                            new(115, date.AddMonths(-1)),
                            new(131, date.AddDays(-15)),
                        })
                };

                var expectedThisMonth = new WikiPageStats[]
                {
                    new("/Foo",
                        1,
                        new WikiPageStats.Stat[]
                        {
                            new(201, date.AddDays(-14)),
                            new(212, date.AddDays(-3)),
                            new(213, date.AddDays(-2)),
                            new(214, date.AddDays(-1)),
                            new(215, date)
                        }),
                    new("/Bar",
                        2,
                        new WikiPageStats.Stat[]
                        {
                            new(201, date.AddDays(-14)),
                            new(215, date),
                            new(216, date.AddDays(1)),
                            new(217, date.AddDays(2)),
                            new(228, date.AddDays(13))
                        })
                };
                return (pageStats, expectedLastMonth, expectedThisMonth);
            }
        }
    }
}