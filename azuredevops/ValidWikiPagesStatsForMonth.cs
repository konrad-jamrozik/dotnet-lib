using System;
using System.Collections.Generic;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    public record ValidWikiPagesStatsForMonth(ValidWikiPagesStats Stats) 
        : ValidWikiPagesStats(ValidStatsForMonth(Stats))
    {
        public ValidWikiPagesStatsForMonth(IEnumerable<WikiPageStats> stats, DateDay startDay, DateDay endDay)
            : this(new ValidWikiPagesStats(stats, startDay, endDay)) { }

        public ValidWikiPagesStatsForMonth(IEnumerable<WikiPageStats> stats, DateMonth month) 
            // kj2 I need to introduce some type like DaySpan, which is a pair of (DateDay start, DateDay end)
            // use it here (instead of passing the two days separately do month.DaySpan)
            // in many places including computations that do things like "-pageViewsForDays+1".
            // In fact, pageViewsForDays should be of that type itself.
            // kj2 also: rename existing DayRange substrings to DaySpan.
            : this(new ValidWikiPagesStats(stats, month.FirstDay, month.LastDay)) { }

        public new ValidWikiPagesStatsForMonth Trim(DateTime currentDate, int daysFrom, int daysTo)
            => new ValidWikiPagesStatsForMonth(
                Trim(
                    currentDate.AddDays(daysFrom),
                    currentDate.AddDays(daysTo)));

        public DateMonth Month => Stats.StartDay.AsDateMonth();

        public bool DaySpanIsForEntireMonth
            => Stats.StartDay == Month.FirstDay
               && Stats.EndDay == Month.LastDay;

        private static ValidWikiPagesStats ValidStatsForMonth(ValidWikiPagesStats stats)
        {
            Contract.Assert(stats.DaySpanIsWithinOneMonth());
            return stats;
        }
    }
}