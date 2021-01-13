using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public class SimulatedAdoApi : IAdoApi
    {
        public Task<List<IWikiPageStats>> GetWikiPagesStats(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays)
        {
            // kja to implement
            return Task.FromResult(new List<IWikiPageStats>());
        }
    }
}