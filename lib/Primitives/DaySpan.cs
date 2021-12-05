using System;

namespace Wikitools.Lib.Primitives
{
    public record DaySpan(DateDay SinceDay, DateDay UntilDay)
    {
        public bool Contains(DateTime date)
            => SinceDay.CompareTo(date) <= 0 && 0 <= UntilDay.CompareTo(date);
    }
}