using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;
using Wikitools.Lib.Tests.Json;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools.AzureDevOps.Tests
{
    [TestFixture]
    public class AdoWikiWithStorageTests
    {
        // kja simulated tests
        // - DONE Everything empty: no data from wiki, no data from storage
        // - DONE Data in wiki, nothing in storage
        // - DONE Data in storage, nothing in wiki
        // - DONE Data in wiki and storage, within wiki max limit of 30 days
        // - TO-DO wiki integration tests: see todos below
        // - TO-DO Storage with data from 3 months <- this test should fail until more than 30 pageViewsForDays is properly supported
        [Test]
        public async Task NoData()
        {
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new SimulatedTimeline().UtcNow;
            var adoWiki            = new SimulatedAdoWiki(WikiPageStats.EmptyArray);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = Storage(utcNow, storageDir);
            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = 30;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(new string[0], pagesStats).Assert();
        }

        [Test]
        public async Task DataInWiki()
        {
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new SimulatedTimeline().UtcNow;
            var pagesStatsData     = ValidWikiPagesStatsFixture.PagesStats(new DateDay(utcNow));
            var adoWiki            = new SimulatedAdoWiki(pagesStatsData);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = Storage(utcNow, storageDir);
            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = 30;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(pagesStatsData, pagesStats).Assert();
        }

        [Test]
        public async Task DataInStorage()
        {
            var fs             = new SimulatedFileSystem();
            var utcNow         = new SimulatedTimeline().UtcNow;
            var pagesStatsData = ValidWikiPagesStatsFixture.PagesStats(new DateDay(utcNow));
            var adoWiki        = new SimulatedAdoWiki(WikiPageStats.EmptyArray);
            var storageDir     = fs.NextSimulatedDir();
            var storage        = await Storage(utcNow, storageDir).DeleteExistingAndSave(pagesStatsData, utcNow);

            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = 30;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(pagesStatsData, pagesStats).Assert();
        }

        [Test]
        public async Task DataInWikiAndStorageWithinSameMonth()
        {
            var fs                 = new SimulatedFileSystem();
            var utcNow             = new SimulatedTimeline().UtcNow;
            var currStats = ValidWikiPagesStatsFixture.PagesStats(new DateDay(utcNow));
            // kja off by one bug: this test should not pass on -27. Biggest OK value should be -26, because the underlying
            // data fixture goes 3 days into the past, so -27-3 = -30, so 31 days in the past.
            // For test capturing this bug, see Wikitools.AzureDevOps.Tests.AdoWikiWithStorageTests.DataInStorageOffByOneBug 
            var prevStats  = ValidWikiPagesStatsFixture.PagesStats(new DateDay(utcNow.AddDays(-27)));
            var expectedPagesStats = prevStats.Merge(currStats);
            var adoWiki            = new SimulatedAdoWiki(currStats);
            var storageDir         = fs.NextSimulatedDir();
            var storage            = await Storage(utcNow, storageDir).DeleteExistingAndSave(prevStats, utcNow);

            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storage);
            var pageViewsForDays   = 30;

            // Act
            var pagesStats = await adoWikiWithStorage.PagesStats(pageViewsForDays);

            new JsonDiffAssertion(expectedPagesStats, pagesStats).Assert();
        }

        [Test]
        public async Task DataInStorageOffByOneBug()
        {
            var fs               = new SimulatedFileSystem();
            var utcNow           = new DateDay(new SimulatedTimeline().UtcNow);
            // implicit assumption that the underlying fixture data is going back at least up to 3 days.
            var storedStats      = ValidWikiPagesStatsFixture.PagesStats(utcNow).Trim(utcNow, -3, 0);
            var storageDir       = fs.NextSimulatedDir();
            var storage          = await Storage(utcNow, storageDir).DeleteExistingAndSave(storedStats, utcNow);

            var readStats = storage.PagesStats(pageViewsForDays: 3);

            var expectedStats = storedStats.Trim(utcNow, -2, 0);

            // kja this fails because storage.PagesStats actually pulls days from range of [-3, 0] instead of [-2, 0]
            // Before fixing that, confirm this behaves inline with behavior captured in
            // Wikitools.AzureDevOps.Tests.AdoWikiWithStorageTests.ObtainsAndStoredDataFromWiki
            //
            // Looking at wiki behavior, perhaps actually the correct behavior is not [-2, 0] or even [-3, 0], but
            // [-3, -1]. After I write wiki tests confirming that, I will have to adjust .Trim call in:
            // Wikitools.AzureDevOps.WikiPagesStatsStorage.PagesStats
            // This is weird, because the API page:
            // https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages%20batch/get?view=azure-devops-rest-6.1
            // explicitly states: "It's inclusive of current day."
            new JsonDiffAssertion(expectedStats, readStats).Assert();
        }

        // kja curr work. Finish up the body (e.g. assert against stored stats) and move all the int tests to a separate class,
        // to deduplicate assumptions about the external wiki.
        /// <summary>
        /// This test tests the following:
        /// - ADO API for Wiki can be successfully queried for data
        /// - Querying wiki for 1 day results in it giving data for today only.
        /// - The obtained data can be successfully stored.
        ///
        /// External dependencies:
        /// Same as Wikitools.AzureDevOps.Tests.AdoWikiWithStorageTests.ObtainsAndMergesDataFromAdoWikiApiAndStorage
        /// </summary>
        [Category("integration")]
        [Test]
        public async Task ObtainsAndStoredDataFromWiki()
        {
            var fs         = new FileSystem();
            var utcNow     = new Timeline().UtcNow;
            var env        = new Environment();
            var cfg        = WikitoolsConfig.From(fs); // kj2 forbidden dependency: azuredevops-tests should not depend on wikitools
            var storageDir = new Dir(fs, cfg.TestStorageDirPath);
            var storage    = Storage(utcNow, storageDir);
            var adoWiki    = new AdoWiki(cfg.AdoWikiUri, cfg.AdoPatEnvVar, env);

            // kja add following tests:
            // - show that pageViewsForDays = 1 returns empty stats. // not true for most of the day; sometimes true due to ingestion delay
            //   - make the test call ADO API for /Home page only, show that page
            //   - manually check the behavior is the same as with entire wiki list
            // - show that pageViewsForDays = 2 always returns stats for previous day.
            var pageViewsForDays = 2;

            var wikiStats   = await adoWiki.PagesStats(pageViewsForDays: pageViewsForDays);
            storage = await storage.DeleteExistingAndSave(wikiStats, utcNow);

            var firstDayWithAnyVisit = wikiStats.Where(ps => ps.DayStats.Any()).Select(ps => ps.DayStats.Min(ds => ds.Day)).Min();
            var lastDayWithAnyVisit = wikiStats.Where(ps => ps.DayStats.Any()).Select(s => s.DayStats.Max(ds => ds.Day)).Max();
            // kja these checks will fail if the resource wiki was never visited on the min/max days
            Assert.That(new DateDay(DateTime.UtcNow.AddDays(-pageViewsForDays+1)), Is.EqualTo(new DateDay(firstDayWithAnyVisit)));
            Assert.That(new DateDay(DateTime.UtcNow.AddDays(-pageViewsForDays+1)), Is.EqualTo(new DateDay(firstDayWithAnyVisit)));
            Assert.That(new DateDay(DateTime.UtcNow), Is.EqualTo(new DateDay(lastDayWithAnyVisit)));
            
            var storedStats = storage.PagesStats(pageViewsForDays);
            var storedFirstDayWithAnyVisit = storedStats.Where(ps => ps.DayStats.Any()).Select(ps => ps.DayStats.Min(ds => ds.Day)).Min();
            var storedLastDayWithAnyVisit = storedStats.Where(ps => ps.DayStats.Any()).Select(s => s.DayStats.Max(ds => ds.Day)).Max();

            Assert.That(new DateDay(DateTime.UtcNow.AddDays(-pageViewsForDays + 1)), Is.EqualTo(new DateDay(storedFirstDayWithAnyVisit)));
            Assert.That(new DateDay(DateTime.UtcNow), Is.EqualTo(new DateDay(storedLastDayWithAnyVisit)));
        }

        [Category("integration")]
        [Test]
        public async Task ObtainsAndStoredDataFromWiki2()
        {
            var fs      = new FileSystem();
            var env     = new Environment();
            var cfg     = WikitoolsConfig.From(fs); // kj2 forbidden dependency: azuredevops-tests should not depend on wikitools
            var adoWiki = new AdoWiki(cfg.AdoWikiUri, cfg.AdoPatEnvVar, env);

            var utcToday = new DateDay(DateTime.UtcNow);
            var utcYesterday = utcToday.AddDays(-1);

            var wikiStatsFor1Day   = await adoWiki.PagesStats(pageViewsForDays: 1);
            var minDay = FirstDayWithAnyVisit(wikiStatsFor1Day);
            var maxDay = LastDayWithAnyVisit(wikiStatsFor1Day);

            // kja CURR WORK
            Assume.That(minDay, Is.Not.Null);
            Assume.That(maxDay, Is.Not.Null);

            // minDay and maxDay being null mean there is no data for today. This might happen
            // not only when there were no visits today, but also when there were visits but were not yet ingested.
            // For details on the ingestion delay, please see comment on Wikitools.AzureDevOps.AdoWiki.GetAllWikiPagesDetails
            Assert.True(minDay == null || minDay.Equals(utcToday));
            Assert.True(maxDay == null || maxDay.Equals(utcToday));

            var wikiStatsForDays2 = await adoWiki.PagesStats(pageViewsForDays: 2);

            var minDay2 = FirstDayWithAnyVisit(wikiStatsForDays2);
            var maxDay2 = LastDayWithAnyVisit(wikiStatsForDays2);
            
            Assert.That(utcYesterday, Is.EqualTo(minDay2));
            Assert.That(new[] { utcYesterday, utcToday }, Contains.Item(maxDay2));

            var wikiStatsForDays3 = await adoWiki.PagesStats(pageViewsForDays: 3);

            var minDay3 = FirstDayWithAnyVisit(wikiStatsForDays3);
            var maxDay3 = LastDayWithAnyVisit(wikiStatsForDays3);
            Assert.That(utcToday.AddDays(-2),             Is.EqualTo(minDay3));
            Assert.That(new[] { utcYesterday, utcToday }, Contains.Item(maxDay3));
        }

        // kj2 move these 2 methods to ValidWikiPagesStats
        private static DateDay? FirstDayWithAnyVisit(ValidWikiPagesStats stats)
        {
            var minDates = stats
                .Where(ps => ps.DayStats.Any())
                .Select(s => s.DayStats.Min(ds => ds.Day))
                .ToList();
            return minDates.Any() ? new DateDay(minDates.Min()) : null;
        }

        private static DateDay? LastDayWithAnyVisit(ValidWikiPagesStats stats)
        {
            var maxDates = stats
                .Where(ps => ps.DayStats.Any())
                .Select(s => s.DayStats.Max(ds => ds.Day))
                .ToList();
            return maxDates.Any() ? new DateDay(maxDates.Max()) : null;
        }

        /// <summary>
        /// This test tests the following:
        /// - ADO API for Wiki can be successfully queried for data
        /// - The obtained data can be successfully stored
        /// - The stored data is then properly merged into ADO API for wiki data
        ///   when "wiki with storage" is used to obtain ADO wiki data both from
        ///   the API and from the data saved in storage.
        ///
        /// The tested scenario:
        /// 1. Obtain 10 days of page stats from wiki (days 1 to 10)
        /// 2. Obtain 6 days of page stats from wiki (days 5 to 10)
        /// 3. Save to storage page stats days 3 to 6
        /// 4. Obtain 4 days of page stats from "wiki with storage" (days 7 to 10)
        /// 4.1. Assert this corresponds to page stats days of 3 to 10 (storage 3 to 6 merged with API 7 to 10)
        ///
        /// External dependencies:
        /// This test queries whatever wiki is defined in WikitoolsConfig, using PAT read from
        /// Env var also defined in that config.
        /// Thus:
        /// - for this test to work, that wiki has to be accessible by the owner of the PAT
        /// - for this test to exercise meaningful behavior, there has to be recent ongoing, daily activity on
        /// the wiki.
        /// </summary>
        [Category("integration")]
        [Test]
        public async Task ObtainsAndMergesDataFromAdoWikiApiAndStorage()
        {
            var fs         = new FileSystem();
            var utcNow     = new Timeline().UtcNow;
            var env        = new Environment();
            var cfg        = WikitoolsConfig.From(fs); // kj2 forbidden dependency: azuredevops-tests should not depend on wikitools
            var storageDir = new Dir(fs, cfg.TestStorageDirPath);
            var storage    = Storage(utcNow, storageDir);
            var adoWiki    = new AdoWiki(cfg.AdoWikiUri, cfg.AdoPatEnvVar, env);

            // Act 1. Obtain 10 days of page stats from wiki (days 1 to 10)
            var statsForDays1To10 = await adoWiki.PagesStats(pageViewsForDays: 10);

            // Act 2. Obtain 4 days of page stats from wiki (days 7 to 10)
            var statsForDays7To10 = await adoWiki.PagesStats(pageViewsForDays: 4);

            // Act 3. Save to storage page stats days 3 to 6
            var statsForDays3To6 = statsForDays1To10.Trim(utcNow, -7, -4);
            var storageWithStats = await storage.DeleteExistingAndSave(statsForDays3To6, utcNow);

            // Act 4. Obtain last 8 days, with last 4 days of page stats from wiki
            var adoWikiWithStorage = AdoWikiWithStorage(adoWiki, storageWithStats, pageViewsForDaysWikiLimit: 4);
            var statsForDays3To10  = await adoWikiWithStorage.PagesStats(pageViewsForDays: 8);

            // Assert 4.1. Act 4 corresponds to page stats days of 3 to 10
            // (data from storage for days 3 to 6 merged with data from ADO API for days 7 to 10)
            var expected = statsForDays3To6.Merge(statsForDays7To10);
            new JsonDiffAssertion(expected, statsForDays3To10).Assert();
        }

        private static AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            WikiPagesStatsStorage storage,
            int? pageViewsForDaysWikiLimit = null)
            => new(adoWiki, storage, pageViewsForDaysWikiLimit);

        private static WikiPagesStatsStorage Storage(DateTime utcNow, Dir storageDir)
            => new(
                new MonthlyJsonFilesStorage(storageDir),
                utcNow);
    }
}