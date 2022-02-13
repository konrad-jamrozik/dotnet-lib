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
public record AdoWiki(IWikiHttpClient Client, ITimeline Timeline) : IAdoWiki
{
    /// <summary>
    /// The Top value is max on which the API doesn't throw. Determined empirically.
    /// </summary>
    public const int MaxApiTop = 100;

    public AdoWiki(
        string wikiUriStr,
        string patEnvVar,
        IEnvironment env,
        ITimeline timeline) : this(
        IWikiHttpClient.WithExceptionWrapping(new AdoWikiUri(wikiUriStr), patEnvVar, env), 
        timeline) { }

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
        // kja inject WikiHttpClient, so I can test AdoWiki with it simulated
        // Then, stop using SimulatedAdoWiki - instead use SimulatedWikiHttpClient
        var wikiHttpClient   = Client;
        var today            = new DateDay(Timeline.UtcNow);
        var wikiPagesDetails = await wikiPagesDetailsFunc(wikiHttpClient, pvfd);
        var wikiPagesStats   = wikiPagesDetails.Select(WikiPageStats.From);
        // kj2-DaySpan replace the startDay, endDay with this below, but first test it
        // var daySpan = pvfd.AsDaySpanUntil(today);
        // Possible tests:
        // Prove that when getting PagesStats and getting stats for full range of
        // Wikitools.AzureDevOps.PageViewsForDays.Max
        // then (a) the Valid stats range is not larger than that, and
        // (b) no smaller than that. The (b) should be caught by assertions.
        // See also AdoWikiWithStorageTests
        return new ValidWikiPagesStats(wikiPagesStats, 
            // kj2-DaySpan get rid of these -days+1 shenanigans
            startDay: today.AddDays(-pvfd.Value+1), 
            endDay: today);
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