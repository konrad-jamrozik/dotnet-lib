using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Wikitools.AzureDevOps
{
    public record WikiPageStats(string Path, List<int> DayViewCounts)
    {
        public WikiPageStats(WikiPageDetail pageDetail) : this(
            pageDetail.Path,
            pageDetail.ViewStats.Select(dayStat => dayStat.Count).ToList()) { }
    }
}