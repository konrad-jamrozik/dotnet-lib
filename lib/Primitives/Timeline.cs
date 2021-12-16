using System;

namespace Wikitools.Lib.Primitives;

public class Timeline : ITimeline
{
    public DateTime UtcNow => DateTime.UtcNow;

    // kja make it a method on ITimeline
    public DateDay DaysFromUtcNow(int days) => new DateDay(UtcNow).AddDays(days);
}