using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoWikiApi(WikiPageStats[] PagesStats) : IAdoWikiApi
    {
        public Task<ValidWikiPagesStats> WikiPagesStats(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays) =>
            Task.FromResult(new ValidWikiPagesStats(PagesStats));
    }
}