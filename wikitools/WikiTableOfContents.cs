using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record WikiTableOfContents : MarkdownDocument
    {
        // kj2 should pagesStats be forced to be ValidStats? Probably yes.
        public WikiTableOfContents(AdoWikiPagesPaths pagesPaths, Task<IEnumerable<WikiPageStats>> pageStats) : base(
            GetContent(pagesPaths, pageStats)) { }

            // kja 2 next todos on critical path:
            // - Actually obtain pageStats passed to this method
            //   - currently the caller uses dummy empty value
            //   - need to come from more than 30 days, so also from storage -> I just implemented support for it.
            // - Correlate pageStats with filePaths
            // - Display the correlation in the format as "Example desired output" below shows
            // At this point things principally work, but then there are immediate things to fix:
            // - Instead of having to manually correlate the filePaths with pageStats, do so 
            // by a call like: filePathsTrie with { pathToValueMap: dictionary { path -> WikiPageStats for path } }
            // this will ensure that the value returned from the PreorderTraversal below is populated.
            //
            // Example desired output:
            // [/Proj](/Proj) - 5 views  
            // [/Proj/BCDR](/Proj/BCDR) - 5 views  
            // [/Proj/BCDR/BCDR Plan - Handling a Data Center Outage](/Proj/BCDR/BCDR-Plan-%2D-Handling-a-Data-Center-Outage) - 100 views :fire:  
            // [/Proj/BCDR/BCDR Plan - Improvements to Proj](/Proj/BCDR/BCDR-Plan-%2D-Improvements-to-Proj) - 200 views :fire::fire:
            //
            // -------------
            // Notes:
            // - this report knows how to convert path to hyperlinks
            //   - hyperlink conversion probably should be abstracted to be generic: in MarkdownDocument
            // - stats will be used to compute if icons should show: new, active, stale
            // - thresholds for icons passed separately as param, coming from config
            //
            // kja 3 need to handle escaping (write a test for it):
            // path from wiki: "Path": "/1CS v2/TSG: how to disable 1CS v2 in Azure DevOps organization",
            // path from file system: "1CS-v2\TSG%3A-how-to-disable-1CS-v2-in-Azure-DevOps-organization.md"
            // kja 4 also, in general, the formats are currently misaligned. Details:
            // WritesTableOfContentsFromLocalWikiGitClone test returns
            // 1CS-v2.md for the file system, but /1CS for the stats.
            // Note that - has ASCII val of 45 (in python: ord('-')) but period ('.') has 46,.
            // Hence 1CS-v2.md appears before 1CS.md in fs. But stripping '.md' would fix it.
            //
            // -------------
            // kja 3 this ZipMatching will have to be adjusted to handle pages that don't have corresponding page
            // stats.
            // - See the pseudocode below. Name it something like "ZipMatchingLeftJoin".
            // - I should confirm the need of having this with an integration test running on local repo.
            // - IMPORTANT: this requires for pagesStats to be sorted.
            //
            // - Pseudocode:
            // trie.PreorderTraversal().ZipMatching(pagesStatsTask.result,
            //   matcher: ((segments, _, _), wikiPageStats =>
            //      if (FileSystem.SplitPathInverse(segments) == wikiPageStats.Path)
            //          yield (segments, wikiPageStats);
            //          next segments;
            //          next wikiPageStats;
            //      else // missing wikiPageStats for given path segments
            //          yield (segments, empty stats)
            //          next segments;

        private static async Task<object[]> GetContent(
            AdoWikiPagesPaths pagesPaths,
            Task<IEnumerable<WikiPageStats>> pagesStatsTask)
        {
            var pagesStats = pagesStatsTask.Result;
            // kj2 probably sort it earlier and include the sort order need in param type
            pagesStats = pagesStats.OrderBy(ps => ps.Path).ToList();


            var trie = new FilePathTrie(pagesPaths);
            var lines = trie.PreorderTraversal().ZipMatching(
                pagesStats,
                // kj2 dehardcode the "/" for joining segments.
                // The authority here is whatever the ADO API returns (which ends up in wikiPageStats).
                match: (fsPathPart, wikiPageStats)
                    => "/" + string.Join("/", fsPathPart.Segments) == wikiPageStats.Path + ".md",
                selectResult: (pathPart, wikiPageStats)
                    => $"[{wikiPageStats.Path}]({wikiPageStats.Path}) - {wikiPageStats.DayStats.Sum(ds => ds.Count)} views");
            return lines.Cast<object>().ToArray();
        }
    }
}