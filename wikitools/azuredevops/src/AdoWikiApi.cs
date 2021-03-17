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
    public record AdoWikiApi(IOSEnvironment Env) : IAdoWikiApi
    {
        // Max value supported by https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages%20batch/get?view=azure-devops-rest-6.1
        // Confirmed empirically.
        private const int MaxPageViewsForDays = 30;

        public async Task<ValidWikiPagesStats> WikiPagesStats(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays)
        {
            Contract.Assert(pageViewsForDays, nameof(pageViewsForDays), new Range(1, MaxPageViewsForDays), upperBoundReason: "ADO API limit");

            var wikiHttpClient   = WikiHttpClient(adoWikiUri, patEnvVar);
            var wikiPagesDetails = await GetAllWikiPagesDetails(adoWikiUri, pageViewsForDays, wikiHttpClient);
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