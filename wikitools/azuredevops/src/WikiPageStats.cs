using System;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    public record WikiPageStats(string Path, int Id, WikiPageStats.DayStat[] DayStats)
    {
        public static readonly WikiPageStats[] EmptyArray = { };

        public static WikiPageStats From(WikiPageDetail pageDetail) =>
            new(pageDetail.Path, pageDetail.Id, DayStatsFrom(pageDetail));

        private static DayStat[] DayStatsFrom(WikiPageDetail pageDetail) =>
            // Confirmed empirically the dayStat is for UTC view counts, not local time view counts.
            pageDetail.ViewStats?.Select(dayStat => new DayStat(dayStat.Count, dayStat.Day.Utc()))
                .OrderBy(ds => ds.Day)
                .ToArray()
            ?? Array.Empty<DayStat>();

        public record DayStat(int Count, DateTime Day) { }
    }
}