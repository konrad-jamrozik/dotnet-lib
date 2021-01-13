using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Wikitools.AzureDevOps
{
    public class SimulatedAdoApi : IAdoApi
    {
        public Task<List<WikiPageDetail>> GetWikiPagesDetails(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays)
        {
            // kja to implement
            return Task.FromResult(new List<WikiPageDetail>());
        }
    }
}