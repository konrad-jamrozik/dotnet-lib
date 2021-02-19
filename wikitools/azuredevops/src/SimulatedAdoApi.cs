using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoApi(WikiPageStats[] PagesStats) : IAdoApi
    {
        public Task<ValidWikiPagesStats> WikiPagesStats(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays) =>
            Task.FromResult(new ValidWikiPagesStats(PagesStats));
    }
}