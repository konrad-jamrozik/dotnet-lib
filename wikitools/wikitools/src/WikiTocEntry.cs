using Wikitools.AzureDevOps;

namespace Wikitools
{
    public record WikiTocEntry(int Depth, string Path, WikiPageStats Stats)
    {

    }
}