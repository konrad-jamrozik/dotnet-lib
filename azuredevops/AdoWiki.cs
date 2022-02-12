using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
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
public record AdoWiki(AdoWikiUri AdoWikiUri, string PatEnvVar, IEnvironment Env, ITimeline Timeline) : IAdoWiki
{
    public AdoWiki(
        string wikiUriStr,
        string patEnvVar,
        IEnvironment env,
        ITimeline timeline) : this(
        new AdoWikiUri(wikiUriStr),
        patEnvVar,
        env,
        timeline) { }

    public Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pvfd) =>
        // ReSharper disable once ConvertClosureToMethodGroup
        PagesStats(pvfd, (wikiHttpClient, pvfd) => GetWikiPagesDetails(wikiHttpClient, pvfd));

    public Task<ValidWikiPagesStats> PageStats(PageViewsForDays pvfd, int pageId) =>
        PagesStats(pvfd, (wikiHttpClient, pvfd) 
            => GetWikiPagesDetails(wikiHttpClient, pvfd, pageId));

    private async Task<ValidWikiPagesStats> PagesStats(
        PageViewsForDays pvfd,
        Func<IWikiHttpClient, PageViewsForDays, Task<IEnumerable<WikiPageDetail>>>
            wikiPagesDetailsFunc)
    {
        // kja inject WikiHttpClient, so I can test AdoWiki with it simulated
        // Then, stop using SimulatedAdoWiki - instead use SimulatedWikiHttpClient
        var wikiHttpClient   = WikiHttpClient(AdoWikiUri, PatEnvVar);
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
        IWikiHttpClient wikiHttpClient,
        PageViewsForDays pvfd)
    {
        // The Top value is max on which the API doesn't throw. Determined empirically.
        var wikiPagesBatchRequest = new WikiPagesBatchRequest
            { Top = 100, PageViewsForDays = pvfd.ValueWithinAdoApiLimit };
        var wikiPagesDetails = new List<WikiPageDetail>();
        string? continuationToken = null;
        do
        {
            wikiPagesBatchRequest.ContinuationToken = continuationToken;
                
            var wikiPagesDetailsPage = await wikiHttpClient.GetPagesBatchAsync(
                wikiPagesBatchRequest,
                AdoWikiUri.ProjectName,
                AdoWikiUri.WikiName);
            wikiPagesDetails.AddRange(wikiPagesDetailsPage);
            continuationToken = wikiPagesDetailsPage.ContinuationToken;
        } while (continuationToken != null);

        return wikiPagesDetails;
    }

    private async Task<IEnumerable<WikiPageDetail>> GetWikiPagesDetails(
        IWikiHttpClient wikiHttpClient,
        PageViewsForDays pvfd,
        int pageId) =>
        (await wikiHttpClient.GetPageDataAsync(
            AdoWikiUri.ProjectName,
            AdoWikiUri.WikiName,
            pageId,
            pvfd)).WrapInList();

    private WikiHttpClientWithExceptionWrapping WikiHttpClient(AdoWikiUri wikiUri, string patEnvVar)
    {
        // Construction of VssConnection with PAT based on
        // https://docs.microsoft.com/en-us/azure/devops/integrate/get-started/client-libraries/samples?view=azure-devops#personal-access-token-authentication-for-rest-services
        // Linked from https://docs.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops#samples
        VssConnection connection = new(
            new Uri(wikiUri.CollectionUri),
            new VssBasicCredential(string.Empty, password: Env.Value(patEnvVar)));

        // Microsoft.TeamFoundation.Wiki.WebApi Namespace doc:
        // https://docs.microsoft.com/en-us/dotnet/api/?term=Wiki
        var wikiHttpClient = connection.GetClient<WikiHttpClient>();

        return new WikiHttpClientWithExceptionWrapping(wikiHttpClient);
    }
}