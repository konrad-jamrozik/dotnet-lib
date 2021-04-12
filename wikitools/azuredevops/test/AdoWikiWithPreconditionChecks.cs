using System;
using System.Threading.Tasks;

namespace Wikitools.AzureDevOps.Tests
{
    public record AdoWikiWithPreconditionChecks(IAdoWiki AdoWiki) : IAdoWiki
    {
        public async Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays)
        {
            try
            {
                return await AdoWiki.PagesStats(pageViewsForDays);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<ValidWikiPagesStats> PageStats(int pageViewsForDays, int pageId)
        {
            try
            {
                return await AdoWiki.PageStats(pageViewsForDays, pageId);
            }
            catch (Exception e)
            {
                // kja use here exception filters to catch exceptions of types that I will have to introduce:
                // exception for busted PAT token, e.g.:
                //   Microsoft.VisualStudio.Services.Common.VssUnauthorizedException : VS30063: You are not authorized to access https://dev.azure.com.
                //   Data:
                //   CredentialsType: Basic
                // exception for page with given ID not existing, e.g.:
                //   Microsoft.VisualStudio.Services.Common.VssServiceException : The wiki page id '13393000' does not exist.
                // To do this properly I will have to modify AdoWiki type.
                // I will have to inject a new type that will wrap around ADO WikiHttpClient
                // and intercept ADO exceptions and rethrow appropriate type of my own exceptions
                // Observe that currently in Wikitools.AzureDevOps.AdoWiki.PagesStats and PageStats
                // the client is created in Wikitools.AzureDevOps.AdoWiki.WikiHttpClient
                // Now instead of that it will have to be injected (to be precise - its exception-handling wrapper).
                //
                // Need to do the same update for PagesStats method above.
                Console.WriteLine(e);
                throw;
            }
        }
    }
}