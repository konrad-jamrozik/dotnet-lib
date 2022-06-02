using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.AzureDevOps.Tests;

[Category("integration")]
[TestFixture]
public class AdoWikiWithStorageIntegrationTests
{
    /// <summary>
    /// This test tests the following:
    /// - ADO API for Wiki can be successfully queried for single page data
    /// - Querying wiki for 5 days results in it giving data for today
    /// and four days in the past.
    ///   - The today cutoff is UTC. So if today UTC time is 2 AM,
    ///     the stats for today will include only 2 hours.
    /// - The obtained data can be successfully stored and retrieved.
    /// </summary>
    [Test]
    public async Task ObtainsAndStoresDataFromAdoWikiFor5DaysBySinglePage() =>
        await VerifyDaySpanOfWikiStats(
            pvfd: 5,
            statsFromAdoApi: WikiPageStatsForSinglePage);

    [Test]
    public async Task ObtainsAndStoresDataFromAdoWikiFor5DaysByMultiplePages() =>
        await VerifyDaySpanOfWikiStats(
            pvfd: 5,
            statsFromAdoApi: WikiPageStatsForAllPages);

    /// <summary>
    /// This test tests the following:
    /// - ADO API for Wiki can be successfully queried for single page stats
    /// - The obtained data can be successfully stored
    /// - The stored data is then properly merged into wiki data from ADO API,
    ///   when "wiki with storage" is used to retrieve ADO wiki data both from
    ///   the API and from the data saved in storage.
    ///
    /// The tested scenario is explained inline in the code.
    /// </summary>
    [Test]
    public async Task ObtainsAndMergesDataFromAdoWikiApiAndStorage()
    {
        var wikiDecl = new AdoWikiWithStorageDeclare();
        var (pageId, wiki, storage) = ArrangeSut();

        // ReSharper disable CommentTypo
        // Act 1. Obtain 10 days of page stats from wiki (days 1 to 10)
        // WWWWWWWWWW
        var statsForDays1To10 = await wiki.PageStats(pvfd: 10, pageId);

        // Act 2. Obtain 4 days of page stats from wiki (days 7 to 10)
        // ------WWWW
        var statsForDays7To10 = await wiki.PageStats(pvfd: 4, pageId);

        // Act 3. Save to storage page stats for days 3 to 6
        // WWWWWWWWWW
        // ->
        // --SSSS----
        var statsForDays3To6 = statsForDays1To10.Trim(statsForDays1To10.DaySpan.EndDay, -7, -4);
        var storageWithStats = await storage.ReplaceWith(statsForDays3To6);

        // Act 4. Obtain last 8 days (days 3 to 10) of stats,
        // while assuming that wiki can provide stats at most from last 4 days (days 7 to 10)
        // --SSSS----
        // ->
        // --SSSSWWWW
        var wikiWithStorage = wikiDecl.AdoWikiWithStorage(
            wiki,
            storageWithStats,
            pageViewsForDaysMax: 4);
        var statsForDays3To10 = await wikiWithStorage.PageStats(pvfd: 8, pageId);

        // Assert 4.1. Assert data from Act 4 corresponds to page stats days of 3 to 10
        // (data from storage for days 3 to 6 merged with data from ADO API for days 7 to 10)
        // actual:
        // --SSSSWWWW // Act 4, actual
        var actual = statsForDays3To10;
        // == // equals to
        // expected:
        // --SSSS---- // Act 3
        // merged with
        // ------WWWW // Act 2
        var expected = statsForDays3To6.Merge(statsForDays7To10);
        // ReSharper restore CommentTypo
        new JsonDiffAssertion(expected, actual).Assert();
    }

    /// <summary>
    /// Arranges integration tests system under tests and relevant dependencies.
    ///
    /// Notes on the external dependencies used:
    /// - For the wiki to provide meaningful behavior to exercise,
    ///   there has to be recent ongoing, daily activity.
    /// - For other assumptions, see comments on WikitoolsConfig members.
    /// </summary>
    private static (int pageId, IAdoWiki wiki, AdoWikiPagesStatsStorage storage)
        ArrangeSut() // kja ArrangeSut / refactor. This method has too long return type.
    {
        var timeline    = new Timeline();
        var utcNow      = timeline.UtcNow;
        var fs          = new FileSystem();
        var env         = new Environment();
        var cfg         = new Configuration(fs);
        var adoTestsCfg = cfg.Load<IAzureDevOpsTestsCfg>();
        var storageDir  = adoTestsCfg.TestStorageDir(fs);
        var storageDecl = new AzureDevOps.AdoWikiPagesStatsStorageDeclare();
        var storage     = storageDecl.AdoWikiPagesStatsStorage(storageDir, utcNow);

        IAdoWiki wiki = new AdoWiki(
            adoTestsCfg.AzureDevOpsCfg().AdoWikiUri(),
            adoTestsCfg.AzureDevOpsCfg().AdoPatEnvVar(),
            env,
            new DateDay(utcNow));
        wiki = new AdoWikiWithPreconditionChecks(wiki);

        return (adoTestsCfg.TestAdoWikiPageId(), wiki, storage);
    }

    private Task<ValidWikiPagesStats> WikiPageStatsForSinglePage(
        IAdoWiki wiki,
        PageViewsForDays pvfd,
        int pageId)
        => wiki.PageStats(pvfd, pageId);

    private async Task<ValidWikiPagesStats> WikiPageStatsForAllPages(
        IAdoWiki wiki,
        PageViewsForDays pvfd,
        int pageId)
        => (await wiki.PagesStats(pvfd)).WhereStats(stats => stats.Id == pageId);

    private async Task VerifyDaySpanOfWikiStats(
        PageViewsForDays pvfd,
        Func<IAdoWiki, PageViewsForDays, int, Task<ValidWikiPagesStats>> statsFromAdoApi)
    {
        var (pageId, wiki, statsStorage) = ArrangeSut();

        var lastDay = wiki.Today();
        var expectedLastDaySpan = new DaySpan(lastDay.AddDays(-1), lastDay);
        var expectedFirstDay = pvfd.AsDaySpanUntil(lastDay).StartDay;

        // Act: obtain the data from the ADO API for wiki
        var stats = await statsFromAdoApi(wiki, pvfd, pageId);

        // Act: store the data
        statsStorage = await statsStorage.ReplaceWith(stats);

        // Act: read the stored data
        var storedStats = statsStorage.PagesStats(pvfd);

        var actualFirstDay = stats.FirstDayWithAnyView;
        var storedFirstDay = storedStats.FirstDayWithAnyView;
        var actualLastDay  = stats.LastDayWithAnyView;
        var storedLastDay  = storedStats.LastDayWithAnyView;

        // Might be null if:
        // - there were no views to the wiki in the used pageViewsForDays
        // - or there were views but they were not yet ingested.
        // For details on the ingestion delay, please see the comment
        // on Wikitools.AzureDevOps.AdoWiki
        Assert.That(actualFirstDay, Is.Null.Or.AtLeast(expectedFirstDay));
        Assert.That(actualLastDay,  Is.Null.Or.AtMost(expectedLastDaySpan.EndDay));

        Assert.That(storedFirstDay, Is.EqualTo(actualFirstDay));
        Assert.That(storedLastDay,  Is.EqualTo(storedLastDay));

        // Assuming, not asserting, because:
        // - the data might be null, due to reasons explained above.
        // - or nobody might have viewed the wiki on these specific days.
        Assume.That(
            actualFirstDay,
            Is.EqualTo(expectedFirstDay),
            ExactDayAssumptionViolationMessage("Minimum first", pvfd));
        Assume.That(
            actualLastDay,
            Is.AtLeast(expectedLastDaySpan.StartDay),
            ExactDayAssumptionViolationMessage("Maximum last", pvfd));

        string ExactDayAssumptionViolationMessage(string dayType, PageViewsForDays pvfd)
        {
            return $"{dayType} possible day for pageViewsForDays: {pvfd}. " +
                   "Possible lack of views (null) or ingestion delay (see comment on AdoWiki). " +
                   $"UTC time: {DateTime.UtcNow}";
        }
    }

}