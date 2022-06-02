using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Tests.Primitives;

public static class TimelineFixture
{
    public static DateDay CurrentDay
    {
        get
        {
            var timeline = new Timeline();
            var utcNow = timeline.UtcNow;
            return new DateDay(utcNow);
        }
    }
}