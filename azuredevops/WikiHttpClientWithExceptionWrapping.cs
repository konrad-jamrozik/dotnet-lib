﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public record WikiHttpClientWithExceptionWrapping
    (WikiHttpClient Client, AdoWikiUri Uri) : IWikiHttpClient
{
    public int PageViewsForDaysMax() => PageViewsForDays.Max;

    public Task<WikiPageDetail> GetPageDataAsync(PageViewsForDays pvfd, int pageId) =>
        // API reference:
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/page-stats/get?view=azure-devops-rest-6.0
        TryInvoke(
            () => Client.GetPageDataAsync(
                Uri.ProjectName,
                Uri.WikiName,
                pageId,
                pvfd.Days));

    public Task<PagedList<WikiPageDetail>> GetPagesBatchAsync(WikiPagesBatchRequest request) =>
        // API reference:
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages-batch/get?view=azure-devops-rest-6.0
        TryInvoke(() => Client.GetPagesBatchAsync(request, Uri.ProjectName, Uri.WikiName));

    private readonly DateDay _today = new DateDay(new Timeline().UtcNow);

    /// <summary>
    /// Today() represents real-world today, thus reflecting
    /// the behavior of the decorated WikiHttpClient.
    /// </summary>
    DateDay IWikiHttpClient.Today() => _today;

    private async Task<T> TryInvoke<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
        }
        catch (VssUnauthorizedException e) when
            (e.Message.Contains("VS30063: You are not authorized to access https://dev.azure.com"))
        {
            throw new ResourceException(ExceptionCode.Unauthorized, e);
        }
        catch (VssServiceException e) when
            (Regex.Match(e.Message, "The wiki page id '.*' does not exist\\.").Success)
        {
            throw new ResourceException(ExceptionCode.NotFound, e);
        }
        catch (Exception e)
        {
            throw new ResourceException(ExceptionCode.Other, e);
        }
    }
}