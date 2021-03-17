using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record SimulatedAdoWiki(WikiPageStats[] PagesStatsData) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays) =>
            Task.FromResult(new ValidWikiPagesStats(PagesStatsData));
    }
}