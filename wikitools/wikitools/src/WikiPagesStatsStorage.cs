using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public record WikiPagesStatsStorage(IOperatingSystem OS, string StorageDirPath, AdoWiki Wiki)
    {
        public Task<WikiPagesStatsStorage> Update(AdoWiki wiki)
        {
            return Task.FromResult(this);
        }

        public Task<WikiPageStats[]> PagesStats(int pageViewsForDays) 
            => Wiki.PagesStats(pageViewsForDays);
    }
}