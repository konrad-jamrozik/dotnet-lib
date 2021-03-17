using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    // kj2 remove
    public record AdoWiki(IAdoWikiApi AdoWikiApi) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays) =>
            AdoWikiApi.WikiPagesStats(pageViewsForDays);
    }
}