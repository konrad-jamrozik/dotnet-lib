namespace Wikitools.Lib.Primitives;

public static class DaySpanExtensions
{
    public static DaySpan AsDaySpanUntil(this int subject, DateDay endDay)
        => new DaySpan(endDay.AddDays(-subject+1), endDay);
}