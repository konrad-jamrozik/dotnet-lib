using System.Collections.Generic;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    public record ValidWikiPagesStatsForMonth(IEnumerable<WikiPageStats> Stats, DateMonth Month) 
        : ValidWikiPagesStats(CheckInvariants(Stats, Month))
    {
        private static ValidWikiPagesStats CheckInvariants(IEnumerable<WikiPageStats> stats, DateMonth month)
        {
            var validWikiPagesStats = new ValidWikiPagesStats(stats);
            Contract.Assert(validWikiPagesStats.AllVisitedDaysAreInMonth(month));
            return validWikiPagesStats;
        }
    }
}