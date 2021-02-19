using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public interface IAdoApi
    {
        Task<ValidWikiPagesStats> WikiPagesStats(AdoWikiUri adoWikiUri, string patEnvVar, int pageViewsForDays);
    }
}