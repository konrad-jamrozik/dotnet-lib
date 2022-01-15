using System.Threading.Tasks;

namespace Wikitools.AzureDevOps;

public interface IAdoWiki
{
    Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pageViewsForDays);

    Task<ValidWikiPagesStats> PageStats(PageViewsForDays pageViewsForDays, int pageId);
}