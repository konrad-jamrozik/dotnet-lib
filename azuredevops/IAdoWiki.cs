using System.Threading.Tasks;

namespace Wikitools.AzureDevOps;

public interface IAdoWiki
{
    Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pvfd);

    Task<ValidWikiPagesStats> PageStats(PageViewsForDays pvfd, int pageId);
}