using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public interface IAdoApi
    {
        Task<List<WikiPageStats>> GetWikiPagesStats(AdoWikiUri adoWikiUri, string patEnvVar, int pageViewsForDays);
    }
}