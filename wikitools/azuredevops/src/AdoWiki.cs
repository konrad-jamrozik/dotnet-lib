using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record AdoWiki(IAdoApi AdoApi, AdoWikiUri AdoWikiUri, string PatEnvVar)
    {
        public Task<WikiPageStats[]> PagesStats(int pageViewsForDays) =>
            AdoApi.WikiPagesStats(AdoWikiUri, PatEnvVar, pageViewsForDays);
    }
}