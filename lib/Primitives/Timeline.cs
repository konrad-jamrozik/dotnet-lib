using System;

namespace Wikitools.Lib.Primitives;

public class Timeline : ITimeline
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateDay Today => new DateDay(UtcNow);

    public DateDay DaysFromUtcNow(int days) => new DateDay(UtcNow).AddDays(days);
}