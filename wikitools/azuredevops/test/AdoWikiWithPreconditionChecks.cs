using System;
using System.Threading.Tasks;

namespace Wikitools.AzureDevOps.Tests
{
    public record AdoWikiWithPreconditionChecks(IAdoWiki AdoWiki) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays)
        {
            try
            {
                return AdoWiki.PagesStats(pageViewsForDays);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Task<ValidWikiPagesStats> PageStats(int pageViewsForDays, int pageId)
        {
            try
            {
                return AdoWiki.PageStats(pageViewsForDays, pageId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}