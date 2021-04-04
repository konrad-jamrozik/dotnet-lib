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
            AdoWikiPagesStatsStorage storage,
            int? pageViewsForDaysWikiLimit = null) =>
            decl.AdoWikiWithStorage(adoWiki, storage, pageViewsForDaysWikiLimit);

        public AdoWikiPagesStatsStorage Storage(DateTime utcNow, Dir storageDir) =>
            decl.AdoWikiPagesStatsStorage(utcNow, storageDir);
    }
}