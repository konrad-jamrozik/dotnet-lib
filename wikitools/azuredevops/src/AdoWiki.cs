using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record AdoWiki(IAdoApi AdoApi, AdoWikiUri AdoWikiUri, string PatEnvVar) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays) =>
            AdoApi.WikiPagesStats(AdoWikiUri, PatEnvVar, pageViewsForDays);
    }
}