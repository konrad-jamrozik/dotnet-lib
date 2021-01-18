using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoApi(WikiPageStats[] PagesStats) : IAdoApi
    {
        public Task<WikiPageStats[]> GetWikiPagesStats(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays) =>
            Task.FromResult(PagesStats);
    }
}