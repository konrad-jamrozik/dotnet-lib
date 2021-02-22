using System.Collections.Generic;

namespace Wikitools.AzureDevOps
{
    public static class WikiPageStatsIEnumerableExtensions
    {
        public static ValidWikiPagesStats Merge(
            this IEnumerable<WikiPageStats> previousStats,
            ValidWikiPagesStats currentStats) => ValidWikiPagesStats.From(previousStats).Merge(currentStats);
    }
}