using System.Collections.Generic;
using System.Linq;
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

        var pageDetail = PagesStatsData
            .Single(pageStats => pageStats.Id == pageId)
            .ToWikiPageDetail();

        return Task.FromResult(pageDetail);
    }

    public Task<PagedList<WikiPageDetail>> GetPagesBatchAsync(WikiPagesBatchRequest request)
    {
        new PageViewsForDays(request.PageViewsForDays ?? 0).AssertPageViewsForDaysRange();

        var pageIndex = int.Parse(request.ContinuationToken ?? "0");
        var dataPage = PagesStatsData.Skip(pageIndex * AdoWiki.MaxApiTop).Take(AdoWiki.MaxApiTop);
        var pageDetailsPage = dataPage.Select(pageStats => pageStats.ToWikiPageDetail());
        bool itemsToPageLeft = PagesStatsData.Count() > (pageIndex + 1) * AdoWiki.MaxApiTop;
        var continuationToken = itemsToPageLeft ? (pageIndex + 1).ToString() : null;

        return Task.FromResult(
            new PagedList<WikiPageDetail>(pageDetailsPage, continuationToken));
    }
}