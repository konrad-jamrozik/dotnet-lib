namespace Wikitools.Lib.Primitives;

public static class DaySpanExtensions
{
    public static DaySpan AsDaySpanUntil(this int daysCount, DateDay endDay)
        => new DaySpan(endDay.AddDays(-daysCount+1), endDay);
}