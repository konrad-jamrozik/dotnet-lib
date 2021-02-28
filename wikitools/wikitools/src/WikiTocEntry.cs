using Wikitools.AzureDevOps;

namespace Wikitools
{
    // kja 2 Depth likely unnecessary - deduce from Path
    // If Depth removed, then perhaps just use 2-tuple
    public record WikiTocEntry(int Depth, string Path, WikiPageStats Stats)
    {

    }
}