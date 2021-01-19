using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public interface IAdoApi
    {
        Task<WikiPageStats[]> WikiPagesStats(AdoWikiUri adoWikiUri, string patEnvVar, int pageViewsForDays);
    }
}