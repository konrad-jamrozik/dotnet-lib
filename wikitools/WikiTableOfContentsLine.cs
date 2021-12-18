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
            => $"{new WikiPageLink(Path)} - {Views} views";
    }
}