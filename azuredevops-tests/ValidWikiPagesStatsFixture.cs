using System.Collections.Generic;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps.Tests
{
    public class ValidWikiPagesStatsFixture
    {
        public ValidWikiPagesStats PagesStats() => PagesStats(Today);

        private static DateDay Today => new DateDay(new SimulatedTimeline().UtcNow);

        public ValidWikiPagesStatsForMonth PagesStatsForMonth(DateDay date)
            => new ValidWikiPagesStatsForMonth(PagesStats(date));

        public ValidWikiPagesStats PagesStats(DateDay date)
        {
            var wikiPageStats = new WikiPageStats[]
            {
                new("/Home", 1, new WikiPageStats.DayStat[]
                {
                    new(1, date.AddDays(-3)),
                    new(1, date.AddDays(-2)),
                    new(20, date.AddDays(-1)),
                    new(2, date)
                }),
                new("/Foo", 2, new WikiPageStats.DayStat[]
                {
                    new(1, date.AddDays(-3)),
                    new(8, date.AddDays(-2)),
                    new(1, date.AddDays(-1)),
                    new(60, date)
                }),
                new("/Foo/Bar", 3, new WikiPageStats.DayStat[]
                {
                    new(1, date.AddDays(-3)),
                    new(1, date.AddDays(-2)),
                    new(8, date.AddDays(-1)),
                    new(6, date)
                }),
                new("/Foo/Baz", 4, new WikiPageStats.DayStat[]
                {
                    new(100, date.AddDays(-3)),
                    new(80, date.AddDays(-2)),
                    new(1, date.AddDays(-1)),
                    new(1, date)
                }),
                new("/Qux/Quux/Quuz", 5, new WikiPageStats.DayStat[]
                {
                    new(7, date.AddDays(-3)),
                    new(7, date.AddDays(-2)),
                    new(7, date.AddDays(-1)),
                    new(7, date)
                })
            };
            return Build(wikiPageStats);
        }

        public static ValidWikiPagesStats Build(
            IEnumerable<WikiPageStats> pageStats,
            DateDay? currentDay = null)
            => new ValidWikiPagesStats(
                stats: pageStats,
                startDay: ValidWikiPagesStats.FirstDayWithAnyVisitStatic(pageStats) 
                                    ?? Today,
                endDay: ValidWikiPagesStats.LastDayWithAnyVisitStatic(pageStats) 
                                  ?? currentDay 
                                  ?? Today);

        public static ValidWikiPagesStatsForMonth BuildForMonth(
            IEnumerable<WikiPageStats> pageStats)
            => new ValidWikiPagesStatsForMonth(Build(pageStats));
    }
}