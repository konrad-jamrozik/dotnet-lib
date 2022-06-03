using Wikitools.Lib.OS;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps;

public class AdoWikiPagesStatsStorageDeclare
{
    public AdoWikiPagesStatsStorage New(Dir storageDir)
        => new AdoWikiPagesStatsStorage(
            new MonthlyJsonFilesStorage(storageDir));
}