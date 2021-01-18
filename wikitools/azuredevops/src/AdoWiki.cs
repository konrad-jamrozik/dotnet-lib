using System;
using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record AdoWiki(IAdoApi AdoApi, AdoWikiUri AdoWikiUri, string PatEnvVar)
    {
        public async Task<WikiPageStats[]> PagesStats(int PageViewsForDays)
        {
            WikiPageStats[] pagesStats = await AdoApi.GetWikiPagesStats(AdoWikiUri, PatEnvVar, PageViewsForDays);
            return pagesStats;
        }

    }
}