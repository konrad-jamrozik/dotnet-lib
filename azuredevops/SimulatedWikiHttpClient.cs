using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;

namespace Wikitools.AzureDevOps;

public record SimulatedWikiHttpClient(
    IEnumerable<WikiPageStats> PagesStatsData) : IWikiHttpClient
{
    public SimulatedWikiHttpClient(ValidWikiPagesStats stats) : this(
        (IEnumerable<WikiPageStats>)stats) { }

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