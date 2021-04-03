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

namespace Wikitools.AzureDevOps
{
    public record AdoWiki(AdoWikiUri AdoWikiUri, string PatEnvVar, IEnvironment Env) : IAdoWiki
    {
        public AdoWiki(string adoWikiUriStr, string patEnvVar, IEnvironment env) : this(
            new AdoWikiUri(adoWikiUriStr), patEnvVar, env) { }

        // Max value supported by https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages%20batch/get?view=azure-devops-rest-6.1
        // Confirmed empirically as of 3/27/2021.
        private const int MaxPageViewsForDays = 30;
        // If 0, the call to ADO wiki API still succeeds, but all returned WikiPageDetail will have null ViewStats.
        // Confirmed empirically as of 3/27/2021.
        private const int MinPageViewsForDays = 1;

        public async Task<ValidWikiPagesStats> PagesStats(
            int pageViewsForDays)
        {
            Contract.Assert(
                pageViewsForDays,
                nameof(pageViewsForDays),
                new Range(MinPageViewsForDays, MaxPageViewsForDays),
                upperBoundReason: "ADO API limit");

            var wikiHttpClient   = WikiHttpClient(AdoWikiUri, PatEnvVar);
            var wikiPagesDetails = await GetAllWikiPagesDetails(AdoWikiUri, pageViewsForDays, wikiHttpClient);
            var wikiPagesStats   = wikiPagesDetails.Select(WikiPageStats.From);
            return new ValidWikiPagesStats(wikiPagesStats);
        }

        private WikiHttpClient WikiHttpClient(AdoWikiUri adoWikiUri, string patEnvVar)
        {
            var pat = Env.Value(patEnvVar);

            // Construction of VssConnection with PAT based on
            // https://docs.microsoft.com/en-us/azure/devops/integrate/get-started/client-libraries/samples?view=azure-devops#personal-access-token-authentication-for-rest-services
            // Linked from https://docs.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops#samples
            VssConnection connection = new(
                new Uri(adoWikiUri.CollectionUri),
                new VssBasicCredential(string.Empty, password: pat));

            // Microsoft.TeamFoundation.Wiki.WebApi Namespace doc:
            // https://docs.microsoft.com/en-us/dotnet/api/?term=Wiki
            return connection.GetClient<WikiHttpClient>();
        }

        /// <remarks>
        /// Empirical tests as of 3/27/2021 show that:
        /// - the wiki page visit ingestion delay is 4-6 hours.
        /// - the dates are counted in UTC.
        /// How tested:
        /// - I created and immediately visited kojamroz_test page on 3/27/2021 9:04 PM PDT.
        /// - The page has shown up immediately in the returned list, but with null visits.
        /// - The visit was not being returned by this call as of 3/28/2021 1:08 AM PDT i.e. 4:04 later.
        /// - But it appeared by 3/28/2021 3:07 AM PDT, i.e. 1:59 later.
        /// - When it appeared, it was counted as 3/8/2021, not 3/7/2021.
        /// Presumably because the dates are in UTC not PDT.
        /// </remarks>
        private static async Task<List<WikiPageDetail>> GetAllWikiPagesDetails(
            AdoWikiUri adoWikiUri,
            int pageViewsForDays,
            WikiHttpClient wikiHttpClient)
        {
            // The Top value is max on which the API doesn't throw. Determined empirically.
            var wikiPagesBatchRequest = new WikiPagesBatchRequest { Top = 100, PageViewsForDays = pageViewsForDays };
            var wikiPagesDetails = new List<WikiPageDetail>();
            string? continuationToken = null;
            do
            {
                wikiPagesBatchRequest.ContinuationToken = continuationToken;
                // API reference:
                // https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages%20batch/get?view=azure-devops-rest-6.0
                var wikiPagesDetailsPage = await wikiHttpClient.GetPagesBatchAsync(
                    wikiPagesBatchRequest,
                    adoWikiUri.ProjectName,
                    adoWikiUri.WikiName);
                wikiPagesDetails.AddRange(wikiPagesDetailsPage);
                continuationToken = wikiPagesDetailsPage.ContinuationToken;
            } while (continuationToken != null);

            return wikiPagesDetails;
        }
    }
}