using System;
using System.Collections.Generic;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public record ValidWikiPagesStatsForMonth(ValidWikiPagesStats Stats) 
    : ValidWikiPagesStats(ValidStatsForMonth(Stats))
{
    public ValidWikiPagesStatsForMonth(IEnumerable<WikiPageStats> stats, DaySpan daySpan)
        : this(new ValidWikiPagesStats(stats, daySpan)) { }

    public ValidWikiPagesStatsForMonth(IEnumerable<WikiPageStats> stats, DateMonth month) 
        // kja I need to introduce some type like DaySpan, which is a pair of (DateDay start, DateDay end)
        // use it here (ALREADY DONE)
        // and in many places including computations that do things like "-pageViewsForDays+1".
        // In fact, pageViewsForDays should be of that type itself.
        // kja also: rename existing DayRange substrings to DaySpan.
        : this(new ValidWikiPagesStats(stats, month.DaySpan)) { }

    public new ValidWikiPagesStatsForMonth Trim(DateTime currentDate, int daysFrom, int daysTo)
        => new ValidWikiPagesStatsForMonth(
            Trim(
                currentDate.AddDays(daysFrom),
                currentDate.AddDays(daysTo)));

    public DateMonth Month => Stats.DaySpan.StartDay.AsDateMonth();

    // kja this should be a method on DaySpan instead
    public bool DaySpanIsForEntireMonth
        => Stats.DaySpan.StartDay == Month.FirstDay
           && Stats.DaySpan.EndDay == Month.LastDay;

    private static ValidWikiPagesStats ValidStatsForMonth(ValidWikiPagesStats stats)
    {
        Contract.Assert(stats.DaySpan.IsWithinOneMonth);
        return stats;
    }
}