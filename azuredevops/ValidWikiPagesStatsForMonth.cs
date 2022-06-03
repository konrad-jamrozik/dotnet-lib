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
        : this(new ValidWikiPagesStats(stats, month.DaySpan)) { }

    // kj2 to remove?
    public new ValidWikiPagesStatsForMonth Trim(DateTime currentDate, int daysFrom, int daysTo)
        => new ValidWikiPagesStatsForMonth(
            Trim(
                currentDate.AddDays(daysFrom),
                currentDate.AddDays(daysTo)));

    public DateMonth Month => Stats.DaySpan.Month;

    public bool DaySpanIsForEntireMonth => Stats.DaySpan.IsExactlyForEntireMonth(Month);

    private static ValidWikiPagesStats ValidStatsForMonth(ValidWikiPagesStats stats)
    {
        Contract.Assert(stats.DaySpan.IsWithinOneMonth);
        return stats;
    }
}