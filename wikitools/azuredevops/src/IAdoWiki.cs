using System.Threading.Tasks;

namespace Wikitools.AzureDevOps
{
    public interface IAdoWiki
    {
        // kja this currently assumes that pageViewsForDays is max 30 days (TODO: test it throws if config says more)
        // but the WikiPageStats with storage should support more than 30, and throw only if it reaches local data storage limit.
        // Probably then should pass some date as param, not days.
        Task<ValidWikiPagesStats> PagesStats(int pageViewsForDays);
    }
}