using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

/// <summary>
/// PageViewsForDays that has represents a DaySpan ending (bound to) at EndDate.
/// </summary>
/// <param name="EndDate"></param>
public record BoundPageViewsForDays(int Value, DateDay EndDate) : PageViewsForDays(Value)
{
    public BoundPageViewsForDays(PageViewsForDays pvfd, DateDay endDate) :
        this(pvfd.Value, endDate) { }

    public DaySpan DaySpan => AsDaySpanUntil(EndDate);
}