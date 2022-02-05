using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public record SimulatedAdoWiki(
    IEnumerable<WikiPageStats> PagesStatsData,
    DaySpan DaySpan) : IAdoWiki
{
    public SimulatedAdoWiki(ValidWikiPagesStats stats) : this(stats, stats.DaySpan) { }

    public Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pvfd)
    {
        pvfd.AssertPageViewsForDaysRange();
        return Task.FromResult(new ValidWikiPagesStats(PagesStatsData, DaySpan));
    }

    public Task<ValidWikiPagesStats> PageStats(PageViewsForDays pvfd, int pageId)
    {
        pvfd.AssertPageViewsForDaysRange();
        return Task.FromResult(
            new ValidWikiPagesStats(PagesStatsData.Where(page => page.Id == pageId), DaySpan));
    }
}