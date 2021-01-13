using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Wikitools.AzureDevOps
{
    public class WikiPageStats : IWikiPageStats
    {
        public WikiPageStats(WikiPageDetail pageDetail)
        {
            Path = pageDetail.Path;
            DayViewCounts = pageDetail.ViewStats.Select(stat => stat.Count).ToList();
        }

        public string Path { get; }
        public List<int> DayViewCounts { get; }
    }
}