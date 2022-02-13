using System;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

/// <summary>
/// Represents stats of an ADO wiki page. Primarily to be be constructed from
/// Microsoft.TeamFoundation.Wiki.WebApi.WikiPageDetail [1]
/// by a call to
/// Wikitools.AzureDevOps.WikiPageStats.From.
///
/// Assumed invariants about the underlying ADO API behavior, confirmed by manual tests:
/// 
/// - DayStats array might be empty, but never null.
/// - A DayStat entry has a Count of at least 1. // kj2 assert DayStat.Count >= 1 in code. It is currently asserted in ValidWikiPageStats.
///   - Thus, in case of a day with no views, the DayStats entry for that day is missing altogether.
/// - All the relevant invariants checked in: Wikitools.AzureDevOps.ValidWikiPagesStats.CheckInvariants
/// - The Path format is of format as codified by Wikitools.AzureDevOps.WikiPageStatsPath
///
/// [1] <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/page-stats/get?view=azure-devops-rest-6.0#wikipagedetail"/>
/// </summary>
public record WikiPageStats(string Path, int Id, WikiPageStats.DayStat[] DayStats)
{
    public static readonly WikiPageStats[] EmptyArray = Array.Empty<WikiPageStats>();

    public static WikiPageStats From(WikiPageDetail pageDetail) =>
        new(pageDetail.Path, pageDetail.Id, DayStatsFrom(pageDetail));

    private static DayStat[] DayStatsFrom(WikiPageDetail pageDetail) =>
        // Using .Utc() as confirmed empirically the dayStat counts views in UTC days, not local time days.
        // For example, if you viewed page at 10 PM PST on day X, it will count
        // towards the day X+1, as 10 PM PST is 6 AM UTC the next day.
        // For details, see comment on Wikitools.AzureDevOps.AdoWiki
        pageDetail.ViewStats?
            .Select(pageStat => new DayStat(pageStat.Count, pageStat.Day.Utc()))
            .OrderBy(ds => ds.Day)
            .ToArray()
        ?? Array.Empty<DayStat>();

    // kj2 Day could be of type DateDay
    public record DayStat(int Count, DateTime Day);
}