using System.Collections.Generic;

namespace Wikitools.AzureDevOps
{
    public static class WikiPageStatsIEnumerableExtensions
    {
        public static ValidWikiPagesStats Merge(
            this IEnumerable<WikiPageStats> previousStats,
            ValidWikiPagesStats currentStats) => new ValidWikiPagesStats(previousStats).Merge(currentStats);
    }
}