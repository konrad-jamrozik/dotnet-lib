using System;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Wikitools.AzureDevOps
{
    public record WikiPageStats(string Path, int Id, WikiPageStats.DayStat[] DayStats)
    {
        public static WikiPageStats From(WikiPageDetail pageDetail) =>
            new(pageDetail.Path, pageDetail.Id, GetStats(pageDetail));

        private static DayStat[] GetStats(WikiPageDetail pageDetail) =>
            // kja empirical tests show that the day stats are already in UTC, but ToUniversalTime() assumes they are in PST.
            // Fix all the data I have by doing search/replace of: T08 -> T00.
            pageDetail.ViewStats?.Select(dayStat => new DayStat(dayStat.Count, dayStat.Day.ToUniversalTime())).OrderBy(ds => ds.Day).ToArray()
            ?? Array.Empty<DayStat>();

        public record DayStat(int Count, DateTime Day) { }

    }
}