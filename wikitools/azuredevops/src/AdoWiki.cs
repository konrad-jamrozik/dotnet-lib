using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    /// <remarks>
    /// Empirical tests as of 3/27/2021 show that when obtaining wiki page stats with calls to
    /// Wikitools.AzureDevOps.AdoWiki.PagesStats or PageStats:
    /// - the wiki page visit ingestion delay is 4-6 hours.
    /// - the dates are counted in UTC.
    /// How I conducted the empirical tests:
    /// - I created and immediately visited kojamroz_test page on 3/27/2021 9:04 PM PDT.
    /// - The page has shown up immediately in the list returned from ADO REST API call, but with null visits.
    /// - The visit was not being returned by this call as of 3/28/2021 1:08 AM PDT i.e. 4h 4m later.
    /// - It appeared by 3/28/2021 3:07 AM PDT, i.e. additional 1h 59m later.
    /// - When it appeared, it was counted as 3/28/2021, not 3/27/2021.
    ///   - Presumably because the dates are in UTC, not PDT.
    /// </remarks>
    public record AdoWiki(AdoWikiUri AdoWikiUri, string PatEnvVar, IEnvironment Env) : IAdoWiki
    {
        public AdoWiki(string adoWikiUriStr, string patEnvVar, IEnvironment env) : this(
            new AdoWikiUri(adoWikiUriStr), patEnvVar, env) { }

        // Max value supported by https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages%20batch/get?view=azure-devops-rest-6.1
        // Confirmed empirically as of 3/27/2021.
        public const int MaxPageViewsForDays = 30;
        // If 0, the call to ADO wiki API still succeeds, but all returned WikiPageDetail will have null ViewStats.
        // Confirmed empirically as of 3/27/2021.
        private const int MinPageViewsForDays = 1;

        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays) =>
            PagesStats(pageViewsForDays, GetWikiPagesDetails);

        public Task<ValidWikiPagesStats> PageStats(int pageViewsForDays, int pageId) =>
            PagesStats(pageViewsForDays, (wikiHttpClient, pageViewsForDays) =>
                GetWikiPagesDetails(wikiHttpClient, pageViewsForDays, pageId));

        private async Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays,
            Func<IWikiHttpClient, int, Task<IEnumerable<WikiPageDetail>>> wikiPagesDetailsFunc)
        {
            Contract.Assert(pageViewsForDays, nameof(pageViewsForDays),
                new Range(MinPageViewsForDays, MaxPageViewsForDays), upperBoundReason: "ADO API limit");

            var wikiHttpClient   = WikiHttpClient(AdoWikiUri, PatEnvVar);
            var wikiPagesDetails = await wikiPagesDetailsFunc(wikiHttpClient, pageViewsForDays);
            var wikiPagesStats   = wikiPagesDetails.Select(WikiPageStats.From);
            // kja current date (end day) here is tricky. It has to match the time ADO API was called. It has to be in UTC, so I need to take into account the hour.
            return new ValidWikiPagesStats(wikiPagesStats);
        }

        private async Task<IEnumerable<WikiPageDetail>> GetWikiPagesDetails(
            IWikiHttpClient wikiHttpClient,
            int pageViewsForDays)
        {
            // The Top value is max on which the API doesn't throw. Determined empirically.
            var wikiPagesBatchRequest = new WikiPagesBatchRequest { Top = 100, PageViewsForDays = pageViewsForDays };
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
            int pageViewsForDays,
            int pageId) =>
            (await wikiHttpClient.GetPageDataAsync(
                AdoWikiUri.ProjectName,
                AdoWikiUri.WikiName,
                pageId,
                pageViewsForDays)).AsList();

        private WikiHttpClientWithExceptionWrapping WikiHttpClient(AdoWikiUri adoWikiUri, string patEnvVar)
        {
            // Construction of VssConnection with PAT based on
            // https://docs.microsoft.com/en-us/azure/devops/integrate/get-started/client-libraries/samples?view=azure-devops#personal-access-token-authentication-for-rest-services
            // Linked from https://docs.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops#samples
            VssConnection connection = new(
                new Uri(adoWikiUri.CollectionUri),
                new VssBasicCredential(string.Empty, password: Env.Value(patEnvVar)));

            // Microsoft.TeamFoundation.Wiki.WebApi Namespace doc:
            // https://docs.microsoft.com/en-us/dotnet/api/?term=Wiki
            var wikiHttpClient = connection.GetClient<WikiHttpClient>();

            return new WikiHttpClientWithExceptionWrapping(wikiHttpClient);
        }
    }
}