using System;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Wikitools.AzureDevOps
{
    public record WikiPageStats(string Path, int Id, WikiPageStats.DayStat[] Stats) // kja Stats -> DayStats
    {
        public static WikiPageStats From(WikiPageDetail pageDetail) =>
            new(pageDetail.Path, pageDetail.Id, GetStats(pageDetail));

        private static DayStat[] GetStats(WikiPageDetail pageDetail) =>
            pageDetail.ViewStats?.Select(dayStat => new DayStat(dayStat.Count, dayStat.Day.ToUniversalTime())).ToArray()
            ?? Array.Empty<DayStat>();

        public record DayStat(int Count, DateTime Day) { }

    }
}