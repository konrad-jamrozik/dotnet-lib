using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public record WikiTableOfContents : MarkdownDocument
    {
        public WikiTableOfContents(AdoWikiPagesPaths pagesPaths, Task<IEnumerable<WikiPageStats>> pageStats) : base(
            GetContent(pagesPaths, pageStats)) { }

        private static async Task<object[]> GetContent(
            AdoWikiPagesPaths pagesPaths,
            Task<IEnumerable<WikiPageStats>> pagesStatsTask)
        {
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

            // Notes:
            // - this report knows how to convert path to hyperlink
            //   - hyperlink conversion probably should be abstracted to be generic: in MarkdownDocument
            // - stats will be used to compute if icons should show: new, active, stale
            // - thresholds for icons passed separately as param, coming from config

            // kja 4 I need to implement abstract "ZipMatching" and use it here.
            // It takes as input two sorted collections and zips through them until it finds matching elements,
            // then does the zipping operation, then continues matching.
            // See if MoreLinq has it.
            // IMPORTANT: this requires for pagesStats to be sorted.
            //
            // Pseudocode:
            // trie.PreorderTraversal().ZipMatching(pagesStatsTask.result,
            //   matcher: ((segments, _, _), wikiPageStats =>
            //      if (FileSystem.SplitPathInverse(segments) == wikiPageStats.Path)
            //          yield (segments, wikiPageStats);
            //          next segments;
            //          next wikiPageStats;
            //      else // missing wikiPageStats for given path segments
            //          yield (segments, empty stats)
            //          next segments;

            var trie = new FilePathTrie(pagesPaths);
            var paths = trie.PreorderTraversal().Select(
                pathPart =>
                {
                    var (segments, value, suffixes) = pathPart;
                    return string.Join("/", segments);
                }).Cast<object>().ToArray();
            return paths;
        }
    }
}