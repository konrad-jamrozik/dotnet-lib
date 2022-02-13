using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;

namespace Wikitools.AzureDevOps;

// kja replace SimulatedAdoWiki with SimulatedWikiHttpClient:
// Currently I am using SimulatedAdoWiki instead of this class.
// AdoWiki depends on WikiHttpClient, so using SimulatedAdoWiki means weaker tests.
// In particular, if I would replace SimulatedAdoWiki with this, the
// AdoWikiWithStorageTests would also exercise AdoWiki,
// including the arithmetic logic for PageViewsForDays.
//
// The main challenge of migration of the simulations is data:
// I have various hardcoded fixtures for SimulatedAdoWiki input data;
// I need to convert thm to WikiPageDetails lists
// I also need to implement continuation token.
public record SimulatedWikiHttpClient(
    IEnumerable<WikiPageStats> PagesStatsData) : IWikiHttpClient
{
    public Task<WikiPageDetail> GetPageDataAsync(int pageId, PageViewsForDays pvfd)
    {
        pvfd.AssertPageViewsForDaysRange();
        return Task.FromResult(new WikiPageDetail(0, ""));
    }

    public Task<PagedList<WikiPageDetail>> GetPagesBatchAsync(WikiPagesBatchRequest request)
    {
        new PageViewsForDays(request.PageViewsForDays ?? 0).AssertPageViewsForDaysRange();
        var wikiPageDetails = new List<WikiPageDetail> {new WikiPageDetail(0, "")};
        return Task.FromResult(
            new PagedList<WikiPageDetail>(wikiPageDetails, null));
    }
}