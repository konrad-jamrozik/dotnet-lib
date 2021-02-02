using System;

namespace Wikitools.Lib.Primitives
{
    public class SimulatedTimeline : ITimeline
    {
        public DateTime UtcNow { get; } = new DateTime(year: 2021, month: 1, day: 8).ToUniversalTime();
    }
}