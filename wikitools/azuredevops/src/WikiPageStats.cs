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

        public static WikiPageStats FixNulls(WikiPageStats stats) => stats;
            // ReSharper disable once ConstantNullCoalescingCondition
            // reason: this method exist to fix this null, caused by System.Text.Json.JsonSerializer.Deserialize.
            // kja disabled temporarily. This probably is not actually needed; it was null because I had wrong property key.
            // Also delete From if not needed.
            //stats with { DayStats = stats.DayStats ?? Array.Empty<DayStat>() };
    }
}