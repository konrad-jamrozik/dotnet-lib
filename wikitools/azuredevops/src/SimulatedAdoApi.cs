using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public class SimulatedAdoApi : IAdoApi
    {
        private readonly List<WikiPageStats> _pageStats;

        public SimulatedAdoApi(List<WikiPageStats> pageStats) => _pageStats = pageStats;

        public Task<List<WikiPageStats>> GetWikiPagesStats(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays) =>
            Task.FromResult(_pageStats);
    }
}