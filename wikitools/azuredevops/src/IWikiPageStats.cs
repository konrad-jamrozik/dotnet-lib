using System.Collections.Generic;

namespace Wikitools.AzureDevOps
{
    public interface IWikiPageStats
    {
        string Path { get; }
        List<int> DayViewCounts { get; }
    }
}