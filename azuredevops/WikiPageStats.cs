using System;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

/// <summary>
/// Represents stats of an ADO wiki page. Primarily to be constructed from
/// Microsoft.TeamFoundation.Wiki.WebApi.WikiPageDetail [1]
/// by a call to
/// Wikitools.AzureDevOps.WikiPageStats.From.
///
/// Assumed invariants about the underlying ADO API behavior, confirmed by manual tests:
/// 
/// - DayStats array might be empty, but never null.
/// - A DayStat entry has a Count of at least 1 (this is captured by DayStat ctor)
///   - Consequently, in case of a day with no views, the DayStats entry for that day is missing altogether.
/// - All the relevant invariants checked in: Wikitools.AzureDevOps.ValidWikiPagesStats.CheckInvariants
/// - The Path format is of format as codified by Wikitools.AzureDevOps.WikiPageStatsPath
///
/// [1] https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/page-stats/get?view=azure-devops-rest-6.0#wikipagedetail
/// </summary>
public record WikiPageStats(string Path, int Id, WikiPageStats.DayStat[] DayStats)
{
    public static readonly WikiPageStats[] EmptyArray = Array.Empty<WikiPageStats>();

    public static WikiPageStats From(WikiPageDetail pageDetail) =>
        new(pageDetail.Path, pageDetail.Id, DayStatsFrom(pageDetail));

    public WikiPageDetail ToWikiPageDetail()
        => new WikiPageDetail(Id, Path)
        {
            ViewStats = DayStats.Select(dayStat => new WikiPageStat(dayStat.Day, dayStat.Count))
        };

    private static DayStat[] DayStatsFrom(WikiPageDetail pageDetail) =>
        // Using .Utc() as confirmed empirically the dayStat counts views in UTC days, not local time days.
        // For example, if you viewed page at 10 PM PST on day X, it will count
        // towards the day X+1, as 10 PM PST is 6 AM UTC the next day.
        // For details, see comment on Wikitools.AzureDevOps.AdoWiki
        pageDetail.ViewStats?
            .Select(pageStat => new DayStat(pageStat.Count, new DateDay(pageStat.Day.Utc())))
            .OrderBy(ds => ds.Day)
            .ToArray()
        ?? Array.Empty<DayStat>();

    public class DayStat
    {
        public int Count { get; }
        public DateDay Day { get; }

        public DayStat(int count, DateDay day)
        {
            Contract.Assert(
                count >= 1,
                $"Wiki page day visit count has to be >= 1. Instead, it is {count}.");
            Count = count;
            Day = day;
        }

        private bool Equals(DayStat other)
            => Count == other.Count && Day.Equals(other.Day);

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DayStat)obj);
        }

        public override int GetHashCode()
            => HashCode.Combine(Count, Day);
    }
}