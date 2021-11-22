using System;
using Wikitools.Lib.OS;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps
{
    public class AdoWikiPagesStatsStorageDeclare
    {
        public AdoWikiPagesStatsStorage AdoWikiPagesStatsStorage(Dir storageDir, DateTime utcNow)
            => new AdoWikiPagesStatsStorage(
                new MonthlyJsonFilesStorage(storageDir),
                utcNow);
    }
}