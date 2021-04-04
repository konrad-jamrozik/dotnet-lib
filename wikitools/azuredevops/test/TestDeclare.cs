using System;
using Wikitools.Lib.OS;

namespace Wikitools.AzureDevOps.Tests
{
    public class TestDeclare
    {
        public AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            WikiPagesStatsStorage storage,
            int? pageViewsForDaysWikiLimit = null) =>
            Declare.AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysWikiLimit);

        public WikiPagesStatsStorage Storage(DateTime utcNow, Dir storageDir) =>
            Declare.WikiPagesStatsStorage(utcNow, storageDir);
    }
}