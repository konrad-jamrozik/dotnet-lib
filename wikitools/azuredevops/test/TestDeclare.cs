using System;
using Wikitools.Lib.OS;

namespace Wikitools.AzureDevOps.Tests
{
    public class TestDeclare
    {
        // kj3 decl needs to be composed, same as config
        private readonly Declare decl = new();

        public AdoWikiWithStorage AdoWikiWithStorage(
            IAdoWiki adoWiki,
            WikiPagesStatsStorage storage,
            int? pageViewsForDaysWikiLimit = null) =>
            decl.AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysWikiLimit);

        public WikiPagesStatsStorage Storage(DateTime utcNow, Dir storageDir) =>
            decl.WikiPagesStatsStorage(utcNow, storageDir);
    }
}