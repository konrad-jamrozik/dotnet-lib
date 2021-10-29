using System;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    /// <summary>
    /// Assumed invariants about the underlying ADO API behavior, confirmed by manual tests:
    /// - DayStats array might be empty, but never null.
    /// - A DayStat entry has a Count of at least 1.
    /// - More invariants checked here: Wikitools.AzureDevOps.ValidWikiPagesStats.CheckInvariants
    /// 
    /// </summary>
    public record WikiPageStats(string Path, int Id, WikiPageStats.DayStat[] DayStats)
    {
        public static readonly WikiPageStats[] EmptyArray = { };

        public static WikiPageStats From(WikiPageDetail pageDetail) =>
            new(pageDetail.Path, pageDetail.Id, DayStatsFrom(pageDetail));

        private static DayStat[] DayStatsFrom(WikiPageDetail pageDetail) =>
            // Using .Utc() as confirmed empirically the dayStat counts visits in UTC days, not local time days.
            // For example, if you visited page at 10 PM PST on day X, it will count
            // towards the day X+1, as 10 PM PST is 6 AM UTC the next day.
            // For details, please see comment on Wikitools.AzureDevOps.AdoWiki
            pageDetail.ViewStats?.Select(dayStat => new DayStat(dayStat.Count, dayStat.Day.Utc()))
                .OrderBy(ds => ds.Day)
                .ToArray()
            ?? Array.Empty<DayStat>();

        public record DayStat(int Count, DateTime Day) { }
    }
}