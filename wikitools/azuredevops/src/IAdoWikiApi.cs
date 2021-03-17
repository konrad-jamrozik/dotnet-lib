using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public interface IAdoWikiApi
    {
        Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays);
    }
}