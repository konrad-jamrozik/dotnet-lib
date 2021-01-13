using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Wikitools.AzureDevOps
{
    public interface IAdoApi
    {
        Task<List<WikiPageDetail>> GetWikiPagesDetails(AdoWikiUri adoWikiUri, string patEnvVar, int pageViewsForDays);
    }
}