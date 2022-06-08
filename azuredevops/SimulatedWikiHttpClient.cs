using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public record SimulatedWikiHttpClient(ValidWikiPagesStats PagesStatsData, DateDay Today) : IWikiHttpClient
{
    DateDay IWikiHttpClient.Today() => Today;

    public int PageViewsForDaysMax() => PageViewsForDays.Max;

    public Task<WikiPageDetail> GetPageDataAsync(PageViewsForDays pvfd, int pageId)
    {
        var trimmedStats = PagesStatsData.Trim(pvfd.Days).TrimToPageId(pageId);
        var pageDetail = trimmedStats.Single().ToWikiPageDetail();

        return Task.FromResult(pageDetail);
    }

    public Task<PagedList<WikiPageDetail>> GetPagesBatchAsync(WikiPagesBatchRequest request)
    {
        var pvfd = new PageViewsForDays(request.PageViewsForDays);
        var trimmedStats = PagesStatsData.Trim(pvfd.Days);

        var pageIndex = int.Parse(request.ContinuationToken ?? "0");
        var dataPage = trimmedStats.Skip(pageIndex * AdoWiki.MaxApiTop).Take(AdoWiki.MaxApiTop);
        var pageDetailsPage = dataPage.Select(pageStats => pageStats.ToWikiPageDetail());
        bool itemsToPageLeft = trimmedStats.Count() > (pageIndex + 1) * AdoWiki.MaxApiTop;
        var continuationToken = itemsToPageLeft ? (pageIndex + 1).ToString() : null;

        return Task.FromResult(
            new PagedList<WikiPageDetail>(pageDetailsPage, continuationToken));
    }
}