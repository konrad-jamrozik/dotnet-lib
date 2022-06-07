using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public interface IAdoWiki
{
    Task<ValidWikiPagesStats> PageStats(PageViewsForDays pvfd, int pageId);

    Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pvfd);

    public DateDay Today();
}