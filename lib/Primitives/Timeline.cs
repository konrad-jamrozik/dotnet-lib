using System;

namespace Wikitools.Lib.Primitives
{
    public class Timeline : ITimeline
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}