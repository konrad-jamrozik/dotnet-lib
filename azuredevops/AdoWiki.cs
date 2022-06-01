using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

/// <remarks>
/// Empirical tests as of 3/27/2021 show that when obtaining wiki page stats with calls to
/// Wikitools.AzureDevOps.AdoWiki.PagesStats or PageStats:
/// - the wiki page view ingestion delay is 4-6 hours.
/// - the dates are shown formatted as UTC time zone.
/// How I conducted the empirical tests:
/// - I created and immediately viewed kojamroz_test page on 3/27/2021 9:04 PM PDT.
/// - The page has shown up immediately in the list returned from ADO REST API call, but with null views.
/// - The view was not being returned by this call as of 3/28/2021 1:08 AM PDT i.e. 4h 4m later.
/// - It appeared by 3/28/2021 3:07 AM PDT, i.e. additional 1h 59m later.
/// - When it appeared, it was counted as 3/28/2021, not 3/27/2021.
///   - Presumably because the dates are in UTC, not PDT.
/// </remarks>
public record AdoWiki(IWikiHttpClient Client, DateDay Today) : IAdoWiki
{
    /// <summary>
    /// The Top value is max on which the API doesn't throw. Determined empirically.
    /// </summary>
    public const int MaxApiTop = 100;

    public AdoWiki(
        string wikiUriStr,
        string patEnvVar,
        IEnvironment env,
        DateDay today) : this(
        IWikiHttpClient.WithExceptionWrapping(new AdoWikiUri(wikiUriStr), patEnvVar, env), 
        today) { }

    public Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pvfd) 
        // ReSharper disable once ConvertClosureToMethodGroup
        => PagesStats(pvfd, (wikiHttpClient, pvfd) 
            => GetWikiPagesDetails(wikiHttpClient, pvfd));

    public Task<ValidWikiPagesStats> PageStats(PageViewsForDays pvfd, int pageId)
        => PagesStats(pvfd, (wikiHttpClient, pvfd) 
            => GetWikiPagesDetails(wikiHttpClient, pvfd, pageId));

    private async Task<ValidWikiPagesStats> PagesStats(
        PageViewsForDays pvfd,
        Func<IWikiHttpClient, PageViewsForDays, Task<IEnumerable<WikiPageDetail>>>
            wikiPagesDetailsFunc)
    {
        var wikiHttpClient   = Client;
        var wikiPagesDetails = await wikiPagesDetailsFunc(wikiHttpClient, pvfd);
        var wikiPagesStats   = wikiPagesDetails.Select(WikiPageStats.From);
        return new ValidWikiPagesStats(wikiPagesStats, pvfd, Today);
    }

    private async Task<IEnumerable<WikiPageDetail>> GetWikiPagesDetails(
        IWikiHttpClient wikiClient,
        PageViewsForDays pvfd)
    {
        var wikiPagesBatchRequest = new WikiPagesBatchRequest
            { Top = MaxApiTop, PageViewsForDays = pvfd.ValueWithinAdoApiLimit };
        var wikiPagesDetails = new List<WikiPageDetail>();
        string? continuationToken = null;
        do
        {
            wikiPagesBatchRequest.ContinuationToken = continuationToken;
                
            var wikiPagesDetailsPage = await wikiClient.GetPagesBatchAsync(wikiPagesBatchRequest);
            wikiPagesDetails.AddRange(wikiPagesDetailsPage);
            continuationToken = wikiPagesDetailsPage.ContinuationToken;
        } while (continuationToken != null);

        return wikiPagesDetails;
    }

    private async Task<IEnumerable<WikiPageDetail>> GetWikiPagesDetails(
        IWikiHttpClient wikiClient,
        PageViewsForDays pvfd,
        int pageId) 
        => (await wikiClient.GetPageDataAsync(pageId, pvfd)).WrapInList();
}