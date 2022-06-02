using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps;

public class AdoWikiPagesStatsStorageDeclare
{
    public AdoWikiPagesStatsStorage New(Dir storageDir, DateDay currentDay)
        => new AdoWikiPagesStatsStorage(
            new MonthlyJsonFilesStorage(storageDir),
            currentDay);
}