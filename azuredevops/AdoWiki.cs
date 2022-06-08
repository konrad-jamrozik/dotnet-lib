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
public record AdoWiki(IWikiHttpClient Client) : IAdoWiki
{
    /// <summary>
    /// The Top value is max on which the API doesn't throw. Determined empirically.
    /// </summary>
    public const int MaxApiTop = 100;

    public AdoWiki(
        string wikiUriStr,
        string patEnvVar,
        IEnvironment env) : this(
        Expand(IWikiHttpClient.WithExceptionWrapping(new AdoWikiUri(wikiUriStr), patEnvVar, env))) { }

    private AdoWiki((IWikiHttpClient client, DateDay today) data) : this(data.client) { }

    private static (IWikiHttpClient client, DateDay today) Expand(IWikiHttpClient client)
        => (client, client.Today());

    DateDay IAdoWiki.Today() => Client.Today();

    public int PageViewsForDaysMax() => Client.PageViewsForDaysMax();

    public async Task<ValidWikiPagesStats> PagesStats(int days)
    {
        IEnumerable<WikiPageDetail> wikiPagesDetails = await GetWikiPagesDetails(days);
        return PagesStats(days, wikiPagesDetails);
    }

    public async Task<ValidWikiPagesStats> PageStats(int days, int pageId)
    {
        var wikiPageDetails = await GetWikiPageDetails(days, pageId);
        return PagesStats(days, wikiPageDetails.WrapInList());
    }

    private ValidWikiPagesStats PagesStats(
        int days,
        IEnumerable<WikiPageDetail> wikiPagesDetails)
    {
        var wikiPagesStats = wikiPagesDetails.Select(WikiPageStats.From);
        var daySpan        = days.AsDaySpanUntil(Client.Today());
        return new ValidWikiPagesStats(wikiPagesStats, daySpan);
    }

    private async Task<WikiPageDetail> GetWikiPageDetails(
        int days,
        int pageId)
        => await Client.GetPageDataAsync(days, pageId);

    private async Task<IEnumerable<WikiPageDetail>> GetWikiPagesDetails(PageViewsForDays pvfd)
    {
        var wikiPagesBatchRequest = new WikiPagesBatchRequest
            { Top = MaxApiTop, PageViewsForDays = pvfd.Value };
        var wikiPagesDetails = new List<WikiPageDetail>();
        string? continuationToken = null;
        do
        {
            wikiPagesBatchRequest.ContinuationToken = continuationToken;
                
            var wikiPagesDetailsPage = await Client.GetPagesBatchAsync(wikiPagesBatchRequest);
            wikiPagesDetails.AddRange(wikiPagesDetailsPage);
            continuationToken = wikiPagesDetailsPage.ContinuationToken;
        } while (continuationToken != null);

        return wikiPagesDetails;
    }
}