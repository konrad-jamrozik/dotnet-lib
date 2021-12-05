using System;

namespace Wikitools.Lib.Primitives
{
    public record DaySpan(DateDay AfterDay, DateDay BeforeDay)
    {
        public bool Contains(DateTime date)
            => AfterDay.CompareTo(date) <= 0 && 0 <= BeforeDay.CompareTo(date);
    }
}