using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps.Tests
{
    public static class ValidWikiPagesStatsFixture
    {
        public static ValidWikiPagesStats PagesStats() =>
            PagesStats(new DateDay(new SimulatedTimeline().UtcNow));

        public static ValidWikiPagesStats PagesStats(DateDay date) => new(new WikiPageStats[]
        {
            new("/Home", 1, new WikiPageStats.DayStat[]
            {
                new(0, date.AddDays(-3)), 
                new(0, date.AddDays(-2)), 
                new(20, date.AddDays(-1)), 
                new(1, date)
            }),
            new("/Foo", 2, new WikiPageStats.DayStat[]
            {
                new(0, date.AddDays(-3)), 
                new(8, date.AddDays(-2)), 
                new(0, date.AddDays(-1)), 
                new(60, date)
            }),
            new("/Foo/Bar", 3, new WikiPageStats.DayStat[]
            {
                new(0, date.AddDays(-3)), 
                new(0, date.AddDays(-2)), 
                new(8, date.AddDays(-1)), 
                new(6, date)
            }),
            new("/Foo/Baz", 4, new WikiPageStats.DayStat[]
            {
                new(100, date.AddDays(-3)), 
                new(80, date.AddDays(-2)), 
                new(0, date.AddDays(-1)), 
                new(0, date)
            }),
            new("/Qux/Quux/Quuz", 5, new WikiPageStats.DayStat[]
            {
                new(7, date.AddDays(-3)), 
                new(7, date.AddDays(-2)), 
                new(7, date.AddDays(-1)), 
                new(7, date)
            })
        });
    }
}