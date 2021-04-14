﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Wikitools.AzureDevOps
{
    // kja curr work
    public record WikiHttpClientAdapter(WikiHttpClient Client) : IWikiHttpClient
    {
        public Task<WikiPageDetail> GetPageDataAsync(
            string projectName,
            string wikiName,
            int pageId,
            int pageViewsForDays)
        {

            try // kja abstract away this try in its own method and also wrap it around GetPagesBatchAsync (minus the NotFound).
            {
                // API reference:
                // https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/page%20stats/get?view=azure-devops-rest-6.0
                return Client.GetPageDataAsync(projectName, wikiName, pageId, pageViewsForDays);
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
                throw new ResourceException(ExceptionCode.Unknown, e);
            }
        }

        public Task<PagedList<WikiPageDetail>> GetPagesBatchAsync(
            WikiPagesBatchRequest request,
            string projectName,
            string wikiName)
        {
            try
            {
                // API reference:
                // https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages%20batch/get?view=azure-devops-rest-6.0
                return Client.GetPagesBatchAsync(request, projectName, wikiName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}