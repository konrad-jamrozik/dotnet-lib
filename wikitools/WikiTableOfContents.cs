using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record WikiTableOfContents : MarkdownDocument
    {
        // kj2 should pagesStats be forced to be ValidStats? Probably yes.
        public WikiTableOfContents(
            AdoWikiPagesPaths pagesPaths, 
            Task<IEnumerable<WikiPageStats>> pageStats) 
            : base(GetContent(pagesPaths, pageStats)) { }

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
            var pagesStats = await pagesStatsTask;
            var wikiPathsFromFS = SortPaths(
                pagesPaths.Select(path => (string)WikiPageStatsPath.FromFileSystemPath(path)));
                
            // statsWithoutFsPaths:
            // Currently unused, but later on will be routed to diagnostic logging.
            // Such stats denote pages that have since been deleted.
            // For details, see comment on 
            // Wikitools.AzureDevOps.Tests.ValidWikiPagesStatsTestDataFixture.PageStatsPagesMissing
            var (pathsWithStats, pathsWithoutStats, statsWithoutPaths) =
                wikiPathsFromFS.FullJoinToSegments(
                    pagesStats,
                    firstKeySelector: wikiPath => wikiPath,
                    secondKeySelector: stats => stats.Path);

            pathsWithoutStats = pathsWithoutStats.ToList();
            if (pathsWithoutStats.Any())
                throw new InvariantException(
                    "There should never be any path without wiki stats. "
                    + "Even just-created pages should show up, with 0 views"
                    + "TSG: Ensure your local clone of the ADO wiki is in sync with the remote "
                    + "repository. Offending paths: " + string.Join(";", pathsWithoutStats));

            var tocLines = pathsWithStats.Select(
                data =>
                {
                    var (_, pageStats) = data;
                    return $"[{pageStats.Path}]({ConvertPathToWikiLink(pageStats.Path)}) - " +
                           $"{pageStats.DayStats.Sum(ds => ds.Count)} views";
                });

            return tocLines.Cast<object>().ToArray();
        }

        private static IOrderedEnumerable<string> SortPaths(IEnumerable<string> paths)
        {
            // Here we replace separator with space so that space doesn't interfere
            // with sorting of directories.
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
            return paths.OrderBy(path => path.Replace(WikiPageStatsPath.Separator, " "));
        }

        /// <summary>
        /// Converts the 'path', which is in format
        /// Wikitools.AzureDevOps.WikiPageStatsPath.WikiPageStatsPath,
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