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
            //   - need to come from more than 30 days, so also from storage -> I just implemented support for it, but looks like I still have assertion locking to 30 days.
            //
            // -------------
            // Notes:
            // - stats will be used to compute if icons should show: new, active, stale
            // - thresholds for icons passed separately as param, coming from config
            //
            // -------------
            // kja 3 this ZipMatching will have to be adjusted to handle pages that don't have corresponding stats
            // - See the pseudocode below. Name it something like "ZipMatchingLeftJoin".
            // - I should confirm the need of having this with an integration test running on local repo.
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
            // kj2 document the Separator replace. Its ASCII ord is 47 while e.g. space is 32, which led to weird sorting of paths.
            var pagesStats = (await pagesStatsTask).OrderBy(ps => ps.Path.Replace(WikiPageStatsPath.Separator, " "));
            var convertedFilePaths = pagesPaths
                .Select(path => (string)WikiPageStatsPath.FromFileSystemPath(path))
                .OrderBy(p => p.Replace(WikiPageStatsPath.Separator, " "));

            var lines = convertedFilePaths.ZipMatching(
                pagesStats,
                match: (fsPath, wikiPageStats)
                    => fsPath == wikiPageStats.Path,
                selectResult: (pathPart, wikiPageStats)
                    // kja here the path in () needs to be converted to wiki path format: spaces to dashes, dashes to %2D, () to \(\).
                    => $"[{wikiPageStats.Path}]({ConvertPathToWikiLink(wikiPageStats.Path)}) - {wikiPageStats.DayStats.Sum(ds => ds.Count)} views");
            return lines.Cast<object>().ToArray();
        }

        private static string ConvertPathToWikiLink(string path)
        {
            // kja 5 refactor
            path = path.Replace("-","%2D").Replace(" ","-").Replace("(", @"\(").Replace(")", @"\)");
            return path;
        }
    }
}