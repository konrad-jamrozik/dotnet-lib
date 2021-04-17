using System;
using System.Threading.Tasks;
using NUnit.Framework;

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
            catch (ResourceException e)
            {
                Assert.Fail("Test precondition failure. " +
                            "The test cannot exercise the relevant logic as at least one " +
                            $"of the prerequisites for the test run is not met.\n{e}");
                throw; // Throw to make the compiler happy. Should be unreachable.
            }
        }
    }
}