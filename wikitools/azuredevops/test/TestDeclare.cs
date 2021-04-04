using System;
using Wikitools.Lib.OS;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps.Tests
{
    public class TestDeclare
    {
        public AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            WikiPagesStatsStorage storage,
            int? pageViewsForDaysWikiLimit = null) =>
            new(adoWiki, storage, pageViewsForDaysWikiLimit);

        public WikiPagesStatsStorage Storage(DateTime utcNow, Dir storageDir) =>
            new(
                new MonthlyJsonFilesStorage(storageDir),
                utcNow);
    }
}