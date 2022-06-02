using System;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Wikitools.Lib.OS;

namespace Wikitools.AzureDevOps;

/// <summary>
/// Interface for decorating Microsoft.TeamFoundation.Wiki.WebApi.WikiHttpClient
/// https://docs.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.wiki.webapi.wikihttpclient?view=azure-devops-dotnet
/// </summary>
public interface IWikiHttpClient
{
    Task<WikiPageDetail> GetPageDataAsync(int pageId, PageViewsForDays pvfd);

    Task<PagedList<WikiPageDetail>> GetPagesBatchAsync(WikiPagesBatchRequest request);

    public static WikiHttpClientWithExceptionWrapping WithExceptionWrapping(
        AdoWikiUri wikiUri,
        string patEnvVar,
        IEnvironment env)
    {
        // Construction of VssConnection with PAT based on
        // https://docs.microsoft.com/en-us/azure/devops/integrate/get-started/client-libraries/samples?view=azure-devops#personal-access-token-authentication-for-rest-services
        // Linked from https://docs.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops#samples
        VssConnection connection = new(
            new Uri(wikiUri.CollectionUri),
            new VssBasicCredential(string.Empty, password: env.Value(patEnvVar)));

        // Microsoft.TeamFoundation.Wiki.WebApi Namespace doc:
        // https://docs.microsoft.com/en-us/dotnet/api/?term=Wiki
        var wikiHttpClient = connection.GetClient<WikiHttpClient>();

        return new WikiHttpClientWithExceptionWrapping(wikiHttpClient, wikiUri);
    }
}