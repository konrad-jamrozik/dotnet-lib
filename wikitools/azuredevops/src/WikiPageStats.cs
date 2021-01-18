using System;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Wikitools.AzureDevOps
{
    public record WikiPageStats(string Path, int[] DayViewCounts)
    {
        public WikiPageStats(WikiPageDetail pageDetail) : this(
            pageDetail.Path,
            GetDayViewCounts(pageDetail)) { }

        private static int[] GetDayViewCounts(WikiPageDetail pageDetail)
            => pageDetail.ViewStats?.Select(dayStat => dayStat.Count).ToArray()
               ?? Array.Empty<int>();
    }
}