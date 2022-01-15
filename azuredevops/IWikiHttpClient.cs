using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;

namespace Wikitools.AzureDevOps;

public interface IWikiHttpClient
{
    Task<WikiPageDetail> GetPageDataAsync(
        string projectName,
        string wikiName,
        int pageId,
        PageViewsForDays pageViewsForDays);

    Task<PagedList<WikiPageDetail>> GetPagesBatchAsync(
        WikiPagesBatchRequest request,
        string projectName,
        string wikiName);
}