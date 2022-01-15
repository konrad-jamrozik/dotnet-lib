using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public record SimulatedAdoWiki(
    IEnumerable<WikiPageStats> PagesStatsData,
    DateDay StartDay,
    DateDay EndDay) : IAdoWiki
{
    public SimulatedAdoWiki(ValidWikiPagesStats stats) : this(
        stats, 
        stats.StartDay,
        stats.EndDay)
    {
    }

    public Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pageViewsForDays)
    {
        pageViewsForDays.AssertPageViewsForDaysRange();
        return Task.FromResult(new ValidWikiPagesStats(PagesStatsData, StartDay, EndDay));
    }

    public Task<ValidWikiPagesStats> PageStats(PageViewsForDays pageViewsForDays, int pageId)
    {
        pageViewsForDays.AssertPageViewsForDaysRange();
        return Task.FromResult(
            new ValidWikiPagesStats(PagesStatsData.Where(page => page.Id == pageId), StartDay, EndDay));
    }
}