using System.Linq;
using System.Text.RegularExpressions;
using Wikitools.AzureDevOps;

namespace Wikitools;

public partial record WikiTableOfContents
{
    public record Line(string Path, int Views)
    {
        public Line(WikiPageStats pageStats) : this(
            pageStats.Path,
            pageStats.DayStats.Sum(ds => ds.Count)) { }

        /// <summary>
        /// Constructs Line from a 'line' that was returned from Line.ToString().
        /// </summary>
        public Line(string line) : this(
            PathFromLine(line),
            ViewCountFromLine(line)) { }

        private static string PathFromLine(string line)
            => Regex.Match(line, "^\\[(.*)\\]\\(\\/.*").Groups[1].Value;

        private static int ViewCountFromLine(string line)
            => int.Parse(Regex.Match(line, ".*\\s-\\s(\\d+) views  ").Groups[1].Value);

        public override string ToString()
            => $"[{Path}]({ConvertPathToWikiLink(Path)}) - {Views} views";

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