using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public record AdoWiki(IAdoWikiApi AdoWikiApi, AdoWikiUri AdoWikiUri, string PatEnvVar) : IAdoWiki
    {
        public Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays) =>
            // kj2 instead of passing AdiWikiUri as thi call param, rename AdoWikiApi to AdoWikiApi and pass the uri
            // as ctor param.
            // kj2 Same story with PatEnvVar
            //
            // Fundamentally the problem here is that AdoApi is facade, which is bad, as it forces
            // us to pass union of stuff of which me might mostly not need (think service locator)
            // Converting AdoApi do AdoWikiApi will make it natural to have the URI and patEnvVar
            // passed as params to it. This will mean that the concept of AdoWiki will
            // completely disappear, to be replaced by AdoWikiApi, that can be simulated directly,
            // and the simulation won't need any uri or pat; it will just not have them passed
            // as param at all.
            AdoWikiApi.WikiPagesStats(AdoWikiUri, PatEnvVar, pageViewsForDays);
    }
}