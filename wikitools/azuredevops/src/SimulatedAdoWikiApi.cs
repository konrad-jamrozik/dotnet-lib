using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoWikiApi(WikiPageStats[] PagesStats) : IAdoWikiApi
    {
        public Task<ValidWikiPagesStats> WikiPagesStats(int pageViewsForDays) =>
            Task.FromResult(new ValidWikiPagesStats(PagesStats));
    }
}