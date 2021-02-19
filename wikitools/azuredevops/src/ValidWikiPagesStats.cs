using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    /// <summary>
    /// Represents a collection of ADO wiki page stats.
    ///
    /// Assumed invariants about the underlying ADO API behavior, confirmed by manual tests:
    /// - All dates provided are in UTC.
    /// - For any given page, visit stats for given day appear only once.
    /// - For any given page, it might have an empty array of day visit stats.
    /// - For any given page, the day visit stats are ordered ascending by date.
    /// - For any day visit stats, its Count is int that is 1 or more.
    /// - No page ID will appear more than once.
    /// - As a consequence, for any given page, its (Path, ID) pair is unique within the scope
    ///   of one call to this method.
    /// - If a page path was changed since last call to this method, it will appear only with the new path.
    ///   Consider a page with (ID, Path) of (42, "/Foo") and some set XDayViews of daily view counts.
    ///   Consider following sequence of events:
    ///   1. make first call to this method;
    ///   2. rename the page to /Bar;
    ///   3. make second call to this method.
    ///   In such case, we assume that:
    ///   - the result of the first call will show the page (42, "/Foo") with XDayViews
    ///   - the result of the second call will show (42, "/Bar") with XDayViews, and won't show (42, /"Foo") at all.
    /// - A page with the same path may appear under different ID in consecutive calls to this method.
    ///   - This can happen in case of page rename, as explained above.
    /// 
    /// </summary>
    public class ValidWikiPagesStats
    {
        public ValidWikiPagesStats(IEnumerable<WikiPageStats> stats)
        {
            var statsArr = stats.ToArray();

            statsArr.AssertDistinctBy(ps => ps.Id);
            statsArr.AssertDistinctBy(ps => ps.Path);
            statsArr.ForEach(ps =>
            {
                ps.DayStats.AssertDistinctBy(ds => ds.Day);
                ps.DayStats.AssertOrderedBy(ds => ds.Day);
                ps.DayStats.Assert(ds => ds.Count >= 1);
                ps.DayStats.Assert(ds => ds.Day.Kind == DateTimeKind.Utc);
            });

            Value = statsArr;
        }

        public WikiPageStats[] Value { get; }
    }
}