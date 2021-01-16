using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record GitAuthorsStatsReport2
        (Timeline Timeline, int Days, List<GitAuthorChangeStats> Stats) : IMarkdownDocument
    {
        public List<object> Content =>
            new()
            {
                $"Git contributions since last {Days} days as of {Timeline.UtcNow}",
                "",
                new TabularData2(GetAuthorsChangesStatsRows(Stats))
            };

        public async Task WriteAsync(TextWriter textWriter)
            => await textWriter.WriteAsync(this.ToMarkdown());

        private static (string[] headerRow, object[][] rows) GetAuthorsChangesStatsRows(
            List<GitAuthorChangeStats> authorChangesStats)
        {
            GitAuthorChangeStats[] authorsStatsOrdered = authorChangesStats.SumByAuthor()
                .OrderByDescending(authorStats => authorStats.Insertions + authorStats.Deletions)
                .Where(stats => !stats.Author.Contains("Konrad J"))
                .ToArray()[..5];

            var rows = authorsStatsOrdered
                .Select((s, i) => new object[] { i+1, s.Author, s.FilesChanged, s.Insertions, s.Deletions })
                .ToArray();

            return (headerRow: new[] { "Place", "Author", "Files changed", "Insertions", "Deletions" }, rows);
        }
    }
}