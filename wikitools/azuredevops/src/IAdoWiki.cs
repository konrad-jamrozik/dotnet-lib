using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public interface IAdoWiki
    {
        Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays);
    }
}