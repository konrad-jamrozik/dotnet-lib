using System;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    public record WikiPageStats(string Path, int Id, WikiPageStats.DayStat[] DayStats)
    {
        public static WikiPageStats From(WikiPageDetail pageDetail) =>
            new(pageDetail.Path, pageDetail.Id, DayStatsFrom(pageDetail));

        private static DayStat[] DayStatsFrom(WikiPageDetail pageDetail) =>
            pageDetail.ViewStats?.Select(dayStat => new DayStat(dayStat.Count, dayStat.Day.Utc()))
                .OrderBy(ds => ds.Day)
                .ToArray()
            ?? Array.Empty<DayStat>();

        public record DayStat(int Count, DateTime Day) { }
    }
}