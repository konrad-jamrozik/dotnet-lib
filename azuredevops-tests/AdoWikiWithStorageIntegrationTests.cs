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
    /// - Querying wiki for 1 day results in it giving data for today only.
    ///   - The today cutoff is UTC. So if today today UTC time is 2 AM,
    ///   it will include only 2 hours.
    /// - The obtained data can be successfully stored and retrieved.
    /// </summary>
    [Test]
    public async Task ObtainsAndStoresDataFromAdoWikiForToday() =>
        await VerifyDayRangeOfWikiStats(pageViewsForDays: 1);

    /// <summary>
    /// Like
    /// Wikitools.AzureDevOps.Tests.AdoWikiWithStorageIntegrationTests.ObtainsAndStoresDataFromAdoWikiForToday
    /// but goes four more days into the past.
    /// </summary>
    [Test]
    public async Task ObtainsAndStoresDataFromAdoWikiFor5Days() =>
        await VerifyDayRangeOfWikiStats(pageViewsForDays: 5);

    /// <summary>
    /// This test tests the following:
    /// - ADO API for Wiki can be successfully queried for single and also all pages data
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
        var (wikiDecl, pageId, utcNow, adoWiki, storage) = ArrangeSut();

        // ReSharper disable CommentTypo
        // Act 1. Obtain 10 days of page stats from wiki (days 1 to 10)
        // WWWWWWWWWW
        var statsForDays1To10         = await adoWiki.PagesStats(pageViewsForDays: 10);

        // Act 2. Obtain 4 days of page stats from wiki (days 7 to 10)
        // ------WWWW
        var statsForDays7To10         = await adoWiki.PagesStats(pageViewsForDays: 4);
        var statsForDays7To10For1Page = await adoWiki.PageStats(pageViewsForDays: 4, pageId);

        // Act 3. Save to storage page stats for days 3 to 6
        // WWWWWWWWWW
        // ->
        // --SSSS----
        var statsForDays3To6 = statsForDays1To10.Trim(utcNow, -7, -4);
        var storageWithStats = await storage.ReplaceWith(statsForDays3To6);

        // Act 4. Obtain last 8 days (days 3 to 10), with last 4 days (days 7 to 10) of page stats from wiki
        // --SSSS----
        // ->
        // --SSSSWWWW
        var adoWikiWithStorage = wikiDecl.AdoWikiWithStorage(
            adoWiki,
            storageWithStats,
            pageViewsForDaysMax: 4);
        var statsForDays3To10 = await adoWikiWithStorage.PagesStats(pageViewsForDays: 8);
        var statsForDays3To10For1Page = await adoWikiWithStorage.PageStats(pageViewsForDays: 8, pageId);

        // Assert 4.1. Assert data from Act 4 corresponds to page stats days of 3 to 10
        // (data from storage for days 3 to 6 merged with data from ADO API for days 7 to 10)
        // --SSSSWWWW (Act 4)
        // ==
        // --SSSS---- (Act 3)
        // merged
        // ------WWWW (Act 2)
        // ReSharper restore CommentTypo
        var data = new[]
        {
            (
                expected: statsForDays3To10For1Page.Merge(statsForDays7To10For1Page),
                actual: statsForDays3To10For1Page
            ),
            (
                expected: statsForDays3To6.Merge(statsForDays7To10),
                actual: statsForDays3To10
            )
        };
        data.ForEach(test => new JsonDiffAssertion(test.expected, test.actual).Assert());
    }

    /// <summary>
    /// Arranges integration tests system under tests and relevant dependencies.
    ///
    /// Notes on the external dependencies used:
    /// - For the adoWiki the wiki to provide meaningful behavior to exercise,
    ///   there has to be recent ongoing, daily activity.
    /// - For other assumptions, see comments on WikitoolsConfig members.
    /// </summary>
    private static (AdoWikiWithStorageDeclare wikiDecl, int pageId, DateTime utcNow, IAdoWiki
        adoWiki, AdoWikiPagesStatsStorage storage)
        ArrangeSut() // kj2 ArrangeSut / refactor. This method has too long return type.
    {
        var timeline    = new Timeline();
        var utcNow      = timeline.UtcNow;
        var fs          = new FileSystem();
        var env         = new Environment();
        var cfg         = new Configuration(fs);
        var adoCfg      = cfg.ReadFromAssembly<IAzureDevOpsCfg>();
        var adoTestsCfg = cfg.Read<AzureDevOpsTestsCfg>();
        var storageDir  = new Dir(fs, adoTestsCfg.TestStorageDirPath);
        var wikiDecl    = new AdoWikiWithStorageDeclare();
        var storageDecl = new AzureDevOps.AdoWikiPagesStatsStorageDeclare();
        var storage     = storageDecl.AdoWikiPagesStatsStorage(storageDir, utcNow);

        IAdoWiki adoWiki = new AdoWiki(adoCfg.AdoWikiUri(), adoCfg.AdoPatEnvVar(), env, timeline);
        adoWiki = new AdoWikiWithPreconditionChecks(adoWiki);

        return (wikiDecl, adoTestsCfg.TestAdoWikiPageId, utcNow, adoWiki, storage);
    }

    private async Task VerifyDayRangeOfWikiStats(int pageViewsForDays)
    {
        var (_, pageId, utcNow, adoWiki, statsStorage) = ArrangeSut();

        var expectedLastDay  = new DateDay(utcNow);
        var expectedFirstDay = expectedLastDay.AddDays(-pageViewsForDays+1);

        // Act
        var stats = await adoWiki.PageStats(pageViewsForDays, pageId);

        statsStorage = await statsStorage.ReplaceWith(stats);
        var storedStats = statsStorage.PagesStats(pageViewsForDays);

        var actualFirstDay = stats.FirstDayWithAnyVisit;
        var storedFirstDay = storedStats.FirstDayWithAnyVisit;
        var actualLastDay  = stats.LastDayWithAnyVisit;
        var storedLastDay  = storedStats.LastDayWithAnyVisit;

        // Might be null if:
        // - there were no visits to the wiki in the used pageViewsForDays
        // - or there were visits but they were not yet ingested.
        // For details on the ingestion delay, please see the comment
        // on Wikitools.AzureDevOps.AdoWiki
        Assert.That(actualFirstDay, Is.Null.Or.AtLeast(expectedFirstDay));
        Assert.That(actualLastDay,  Is.Null.Or.AtMost(expectedLastDay));

        Assert.That(storedFirstDay, Is.EqualTo(actualFirstDay));
        Assert.That(storedLastDay,  Is.EqualTo(storedLastDay));

        // Assuming, not asserting, because:
        // - the data might be null, due to reasons explained above.
        // - or nobody might have visited the wiki on these specific days.
        Assume.That(
            actualFirstDay,
            Is.EqualTo(expectedFirstDay),
            ExactDayAssumptionViolationMessage("Minimum first", pageViewsForDays));
        Assume.That(
            actualLastDay,
            Is.EqualTo(expectedLastDay),
            ExactDayAssumptionViolationMessage("Maximum last", pageViewsForDays));

        string ExactDayAssumptionViolationMessage(string dayType, int pageViewsForDays)
        {
            return $"{dayType} possible day for pageViewsForDays: {pageViewsForDays}. " +
                   "Possible lack of visits (null) or ingestion delay (see comment on AdoWiki). " +
                   $"UTC time: {DateTime.UtcNow}";
        }
    }
}