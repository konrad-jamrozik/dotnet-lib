using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

// kja take DaySpan as input instead of StartDay, EndDay
public record SimulatedAdoWiki(
    IEnumerable<WikiPageStats> PagesStatsData,
    DateDay StartDay,
    DateDay EndDay) : IAdoWiki
{
    public SimulatedAdoWiki(ValidWikiPagesStats stats) : this(
        stats, 
        stats.DaySpan.StartDay,
        stats.DaySpan.EndDay)
    {
    }

    public Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pvfd)
    {
        pvfd.AssertPageViewsForDaysRange();
        return Task.FromResult(new ValidWikiPagesStats(PagesStatsData, StartDay, EndDay));
    }

    public Task<ValidWikiPagesStats> PageStats(PageViewsForDays pvfd, int pageId)
    {
        pvfd.AssertPageViewsForDaysRange();
        return Task.FromResult(
            new ValidWikiPagesStats(PagesStatsData.Where(page => page.Id == pageId), StartDay, EndDay));
    }
}