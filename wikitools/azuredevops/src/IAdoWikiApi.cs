using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public interface IAdoWikiApi
    {
        Task<ValidWikiPagesStats> WikiPagesStats(string patEnvVar, int pageViewsForDays);
    }
}