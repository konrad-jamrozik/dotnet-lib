using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public interface IAdoWikiApi
    {
        Task<ValidWikiPagesStats> WikiPagesStats(AdoWikiUri adoWikiUri, string patEnvVar, int pageViewsForDays);
    }
}