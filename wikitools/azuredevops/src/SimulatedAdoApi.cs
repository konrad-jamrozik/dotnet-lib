using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoApi(WikiPageStats[] PagesStats) : IAdoApi
    {
        public Task<WikiPageStats[]> WikiPagesStats(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays) =>
            Task.FromResult(PagesStats);
    }
}