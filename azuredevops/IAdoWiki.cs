using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public interface IAdoWiki
{
    Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pvfd);

    Task<ValidWikiPagesStats> PageStats(PageViewsForDays pvfd, int pageId);

    public DateDay Today();
}