using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.AzureDevOps.Tests
{
    [Category("integration")]
    [TestFixture]
    public class AdoWikiWithStorageIntegrationTests
    {
        // kja curr work. 
        // - make the test call ADO API for /Home page only, show that page
        // - manually check the behavior is the same as with entire wiki list
        // - add Warning/Precondition failure over PAT working or not (need to catch the exception)
        /// <summary>
        /// This test tests the following:
        /// - ADO API for Wiki can be successfully queried for data
        /// - Querying wiki for 1 day results in it giving data for today only.
        /// - The obtained data can be successfully stored.
        /// </summary>
        [Test]
        public async Task ObtainsAndStoresDataFromAdoWiki()
        {
            var decl    = new TestDeclare();
            var utcNow  = new Timeline().UtcNow;
            var (adoWiki, storage) = ArrangeSut(decl, utcNow);

            await VerifyDayRangeOfWikiStats(utcNow, adoWiki, storage, pageViewsForDays: 1);
            await VerifyDayRangeOfWikiStats(utcNow, adoWiki, storage, pageViewsForDays: 2);
        }

        /// <summary>
        /// This test tests the following:
        /// - ADO API for Wiki can be successfully queried for data
        /// - The obtained data can be successfully stored
        /// - The stored data is then properly merged into wiki data from ADO API,
        ///   when "wiki with storage" is used to obtain ADO wiki data both from
        ///   the API and from the data saved in storage.
        ///
        /// The tested scenario is explained inline in the code.
        /// </summary>
        [Test]
        public async Task ObtainsAndMergesDataFromAdoWikiApiAndStorage()
        {
            var decl       = new TestDeclare();
            var utcNow     = new Timeline().UtcNow;
            var (adoWiki, storage) = ArrangeSut(decl, utcNow);

            // ReSharper disable CommentTypo
            // Act 1. Obtain 10 days of page stats from wiki (days 1 to 10)
            // WWWWWWWWWW
            var statsForDays1To10 = await adoWiki.PagesStats(pageViewsForDays: 10);

            // Act 2. Obtain 4 days of page stats from wiki (days 7 to 10)
            // ------WWWW
            var statsForDays7To10 = await adoWiki.PagesStats(pageViewsForDays: 4);

            // Act 3. Save to storage page stats for days 3 to 6
            // WWWWWWWWWW
            // ->
            // --SSSS----
            var statsForDays3To6 = statsForDays1To10.Trim(utcNow, -7, -4);
            var storageWithStats = await storage.OverwriteWith(statsForDays3To6, utcNow);

            // Act 4. Obtain last 8 days (days 3 to 10), with last 4 days (days 7 to 10) of page stats from wiki
            // --SSSS----
            // ->
            // --SSSSWWWW
            var adoWikiWithStorage = decl.AdoWikiWithStorage(adoWiki, storageWithStats, pageViewsForDaysWikiLimit: 4);
            var statsForDays3To10  = await adoWikiWithStorage.PagesStats(pageViewsForDays: 8);

            // Assert 4.1. Assert data from Act 4 corresponds to page stats days of 3 to 10
            // (data from storage for days 3 to 6 merged with data from ADO API for days 7 to 10)
            // --SSSSWWWW (Act 4)
            // ==
            // --SSSS---- (Act 3)
            // merged
            // ------WWWW (Act 2)
            // ReSharper restore CommentTypo
            var expected = statsForDays3To6.Merge(statsForDays7To10);
            new JsonDiffAssertion(expected, statsForDays3To10).Assert();
        }

        /// <summary>
        /// Provides systems under tests to the integration tests: AdoWiki and WikiPagesStatsStorage.
        ///
        /// The adoWiki is defined in WikitoolsConfig, using PAT read from env var also defined in that config.
        /// Thus:
        /// - the PAT needs to provide relevant access to the wiki;
        /// - for the wiki to provide meaningful behavior to exercise, there has to be recent ongoing, daily activity.
        /// </summary>
        private static (AdoWiki adoWiki, WikiPagesStatsStorage storage) ArrangeSut(TestDeclare decl, DateTime utcNow)
        {
            var fs         = new FileSystem();
            var env        = new Environment();
            var cfg        = WikitoolsConfig.From(fs);
            var storageDir = new Dir(fs, cfg.TestStorageDirPath);
            var storage    = decl.Storage(utcNow, storageDir);
            var adoWiki    = new AdoWiki(cfg.AdoWikiUri, cfg.AdoPatEnvVar, env);
            return (adoWiki, storage);
        }

        private async Task VerifyDayRangeOfWikiStats(
            DateTime utcNow,
            IAdoWiki adoWiki,
            WikiPagesStatsStorage statsStorage,
            int pageViewsForDays)
        {
            var expectedLastDay  = new DateDay(utcNow);
            var expectedFirstDay = expectedLastDay.AddDays(-pageViewsForDays+1);

            // Act
            var stats = await adoWiki.PagesStats(pageViewsForDays);

            statsStorage = await statsStorage.OverwriteWith(stats, utcNow);
            var storedStats = statsStorage.PagesStats(pageViewsForDays);

            var actualFirstDay = stats.FirstDayWithAnyVisit;
            var storedFirstDay = storedStats.FirstDayWithAnyVisit;
            var actualLastDay  = stats.LastDayWithAnyVisit;
            var storedLastDay  = storedStats.LastDayWithAnyVisit;

            // Might be null if:
            // - there were no visits to the wiki in the used pageViewsForDays
            // - or there were visits but they were not yet ingested.
            // For details on the ingestion delay, please see the comment
            // on Wikitools.AzureDevOps.AdoWiki.GetAllWikiPagesDetails
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
                       $"Possible lack of visits or ingestion delay. UTC time: {DateTime.UtcNow}";
            }
        }
    }
}