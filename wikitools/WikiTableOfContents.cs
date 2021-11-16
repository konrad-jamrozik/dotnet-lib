using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;

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

        private static async Task<object[]> GetContent(
            AdoWikiPagesPaths pagesPaths,
            Task<IEnumerable<WikiPageStats>> pagesStatsTask)
        {
            var pagesStats = SortPaths(await pagesStatsTask);
            var wikiPathsFromFsPaths = pagesPaths
                .Select(path => (string)WikiPageStatsPath.FromFileSystemPath(path))
                .OrderBy(p => p.Replace(WikiPageStatsPath.Separator, " "));

            IEnumerable<(string? path, WikiPageStats? pagesStats)> fullJoin =
                wikiPathsFromFsPaths.FullJoin(
                    pagesStats,
                    firstKeySelector: path => path,
                    secondKeySelector: stats => stats.Path,
                    firstSelector: path => (path, null),
                    secondSelector: stats => (null, stats),
                    bothSelector: (path, stats) => ((string?)path, (WikiPageStats?)stats)).ToList();

            var lines = fullJoin.Where(data => data.path != null && data.pagesStats != null).Select(
                data =>
                    $"[{data.pagesStats!.Path}]({ConvertPathToWikiLink(data.pagesStats.Path)}) - {data.pagesStats.DayStats.Sum(ds => ds.Count)} views"
            );

            // Currently unused, but later on will be routed to diagnostic logging.
            // Generally speaking this can happen if a path is very new and nobody visitied it yet, but
            // I should confirm it. kj2 pathsWithoutStats 
            var pathsWithoutStats = fullJoin.Where(data => data.path != null && data.pagesStats == null)
                .Select(data => data.path).ToList();
            // Currently unused, but later on will be routed to diagnostic logging.
            // Generally this shouldn't happen, but it does. Looks like my merging algorithm for Valid stats
            // doesn't work as expected. Need to investigate. kj2 statsWithoutFsPaths 
            var statsWithoutFsPaths = fullJoin.Where(data => data.path == null && data.pagesStats != null)
                .Select(data => data.pagesStats).ToList();

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