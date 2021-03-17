using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoWikiApi(WikiPageStats[] PagesStatsData) : IAdoWikiApi
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays) =>
            Task.FromResult(new ValidWikiPagesStats(PagesStatsData));
    }
}