using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps;

public interface IAdoWiki
{
    public DateDay Today();

    Task<ValidWikiPagesStats> PageStats(int days, int pageId);

    Task<ValidWikiPagesStats> PagesStats(int days);
}