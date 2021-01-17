using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public interface IAdoApi
    {
        Task<WikiPageStats[]> GetWikiPagesStats(AdoWikiUri adoWikiUri, string patEnvVar, int pageViewsForDays);
    }
}