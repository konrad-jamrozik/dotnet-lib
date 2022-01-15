using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Wikitools.AzureDevOps.Tests;

public record AdoWikiWithPreconditionChecks(IAdoWiki AdoWiki) : IAdoWiki
{
    public Task<ValidWikiPagesStats> PagesStats(PageViewsForDays pageViewsForDays) => 
        TryInvoke(() => AdoWiki.PagesStats(pageViewsForDays));

    public Task<ValidWikiPagesStats> PageStats(PageViewsForDays pageViewsForDays, int pageId) => 
        TryInvoke(() => AdoWiki.PageStats(pageViewsForDays, pageId));

    private async Task<T> TryInvoke<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
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