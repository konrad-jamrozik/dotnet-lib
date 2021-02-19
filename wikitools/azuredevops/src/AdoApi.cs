using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Wikitools.Lib.Contracts;

namespace Wikitools.AzureDevOps
{
    public class AdoApi : IAdoApi
    {
        public async Task<ValidWikiPagesStats> WikiPagesStats(
            AdoWikiUri adoWikiUri,
            string patEnvVar,
            int pageViewsForDays)
        {
            new Contract(pageViewsForDays, nameof(pageViewsForDays), new Range(1, 30)).Assert();

            var wikiHttpClient   = WikiHttpClient(adoWikiUri, patEnvVar);
            var wikiPagesDetails = await GetAllWikiPagesDetails(adoWikiUri, pageViewsForDays, wikiHttpClient);
            var wikiPagesStats   = wikiPagesDetails.Select(WikiPageStats.From);
            return new ValidWikiPagesStats(wikiPagesStats);
        }

        private static WikiHttpClient WikiHttpClient(AdoWikiUri adoWikiUri, string patEnvVar)
        {
            // kja 3 dehardcode - should be abstracted by OS
            var pat = Environment.GetEnvironmentVariable(patEnvVar);

            // Construction of VssConnection with PAT based on
            // https://docs.microsoft.com/en-us/azure/devops/integrate/get-started/client-libraries/samples?view=azure-devops#personal-access-token-authentication-for-rest-services
            // Linked from https://docs.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops#samples
            VssConnection connection = new(
                new Uri(adoWikiUri.CollectionUri),
                new VssBasicCredential(string.Empty, password: pat));

            // WikiHttpClient source:
            // https://dev.azure.com/mseng/AzureDevOps/_git/AzureDevOps?path=%2FTfs%2FClient%2FWiki%2FGenerated%2FWikiHttpClient.cs&version=GBmaster&line=4635&lineEnd=4635&lineStartColumn=37&lineEndColumn=53&lineStyle=plain&_a=contents
            // Microsoft.TeamFoundation.Wiki.WebApi Namespace Doc:
            // https://docs.microsoft.com/en-us/dotnet/api/?term=Wiki
            return connection.GetClient<WikiHttpClient>();
        }

        // kja strongly type the returned collection, asserting all the constraints on it.
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