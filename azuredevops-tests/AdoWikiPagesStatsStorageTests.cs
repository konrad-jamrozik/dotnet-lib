using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.Tests.Json;
using static Wikitools.Lib.Primitives.SimulatedTimeline;

namespace Wikitools.AzureDevOps.Tests;

[TestFixture]
public class AdoWikiPagesStatsStorageTests
{
    /// <summary>
    /// This test ensures that for "pageViewsForDays", the storage.PagesStats()
    /// correctly returns data for day range:
    ///
    ///   [today - pageViewsForDays + 1, today]
    /// 
    /// e.g. for pageViewsForDays = 3 it will return [today - 2, today]
    /// 
    /// instead of the incorrect:
    /// 
    ///   [today - pageViewsForDays, today]
    /// 
    /// e.g for pageViewsForDays = 3 this would be [today - 3, today].
    ///
    /// This is achieved by arranging in storage stats from day "today-pageViewsForDays"
    /// and then showing that when calling storage.PagesStats(pageViewsForDays)
    /// the returned stats will start from "today-pageViewsForDays+1"
    /// instead of "today-pageViewsForDays".
    /// </summary>
    [Test]
    public async Task FirstDayOfViewsInStorageIsNotOffByOne()
    {
        var pageViewsForDays = new PageViewsForDays(3);
        var pageViewsForDaysSpan = pageViewsForDays.AsDaySpanUntil(UtcNowDay);
        var fixture          = new ValidWikiPagesStatsFixture();
        var stats            = fixture.PagesStatsForMonth(UtcNowDay);

        Assert.That(
            stats.FirstDayWithAnyView,
            Is.EqualTo(pageViewsForDaysSpan.StartDay.AddDays(-1)),
            "Precondition violation: the off by one error won't be detected by this test as " +
            "there are no views in the \"one before expected first\" day in the arranged data.");

        var expectedStats = stats.Trim(pageViewsForDaysSpan);

        Assert.That(
            expectedStats.FirstDayWithAnyView,
            Is.EqualTo(pageViewsForDaysSpan.StartDay),
            "Precondition violation: the expected stats should start exactly at the beginning" +
            "PageViewsForDays day span, otherwise the test won't catch the off by one error.");

        var adoDecl = new AdoWikiPagesStatsStorageDeclare();
        var storage = await adoDecl.AdoWikiPagesStatsStorage(UtcNowDay, stats);

        // Act
        var actualStats = storage.PagesStats(pageViewsForDays);

        new JsonDiffAssertion(expectedStats, actualStats).Assert();
    }
}