using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
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
            var pagesStats = SortPaths(await pagesStatsTask);
            var wikiPathsFromFsPaths = pagesPaths
                .Select(path => (string)WikiPageStatsPath.FromFileSystemPath(path))
                .OrderBy(p => p.Replace(WikiPageStatsPath.Separator, " "));

            var lines = wikiPathsFromFsPaths.ZipMatching(
                pagesStats,
                match: (wikiPath, wikiPageStats)
                    => wikiPath == wikiPageStats.Path,
                selectResult: (wikiPath, wikiPageStats)
                    => $"[{wikiPageStats.Path}]({ConvertPathToWikiLink(wikiPageStats.Path)}) - {wikiPageStats.DayStats.Sum(ds => ds.Count)} views");
            return lines.Cast<object>().ToArray();
        }

        private static IOrderedEnumerable<WikiPageStats> SortPaths(IEnumerable<WikiPageStats> pagesStats)
        {
            return pagesStats
                // Here we replace separator with space so that space doesn't interfere with sorting of directories.
                // ASCII ordinal of the separator is 47, while of space it is 32,
                // so by replacing the separator with the space,
                // space will no longer be sorted first.
                //
                // Without this custom sorting, instead of ending up with paths sorted like this:
                //
                // /dirFoo
                // /dirFoo/file_A1
                // /dirFoo/file_A2
                // /dirFoo Bar/file_B1
                // /dirFoo Bar/file_B2
                //
                // We would end up with paths sorted like this:
                //
                // /dirFoo
                // /dirFoo Bar/file_B1
                // /dirFoo Bar/file_B2
                // /dirFoo/file_A1
                // /dirFoo/file_A2
                // 
                .OrderBy(ps => ps.Path.Replace(WikiPageStatsPath.Separator, " "));
        }

        /// <summary>
        /// Converts the 'path', which is in format Wikitools.AzureDevOps.WikiPageStatsPath.WikiPageStatsPath,
        /// to a link that ADO Wiki understands as an absolute path to a page in given wiki.
        ///
        /// Reference:
        /// See "Supported links for Wiki" / "Absolute path of Wiki pages" in:
        /// <a href="https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops#links" />
        /// </summary>
        private static string ConvertPathToWikiLink(string path)
        {
            path = path.Replace("-","%2D").Replace(" ","-").Replace("(", @"\(").Replace(")", @"\)");
            return path;
        }
    }
}