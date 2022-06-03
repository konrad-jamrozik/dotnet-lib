using System;

namespace Wikitools.Lib.Primitives;

public class SimulatedTimeline : ITimeline
{
    public DateTime UtcNow { get; } = DateTime.SpecifyKind(
        new DateTime(year: 2021, month: 1, day: 8, hour: 11, minute: 15, second: 23),
        DateTimeKind.Utc);

    public readonly DateDay UtcNowDay;

    public SimulatedTimeline()
    {
        UtcNowDay = new DateDay(UtcNow);
    }
}