using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public class SimulatedAdoApi : IAdoApi
    {
        private readonly WikiPageStats[] _pageStats;

        public SimulatedAdoApi(WikiPageStats[] pageStats) => _pageStats = pageStats;

        public Task<WikiPageStats[]> GetWikiPagesStats(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays) =>
            Task.FromResult(_pageStats);
    }
}