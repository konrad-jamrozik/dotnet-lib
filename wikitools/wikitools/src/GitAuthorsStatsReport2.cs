using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record GitAuthorsStatsReport2
        (ITimeline Timeline, int Days, List<GitAuthorChangeStats> Stats) : MarkdownDocument
    {
        public override List<object> Content =>
            new()
            {
                $"Git contributions since last {Days} days as of {Timeline.UtcNow}",
                "",
                new TabularData2(Rows(Stats))
            };

        private static (string[] headerRow, object[][] rows) Rows(
            List<GitAuthorChangeStats> authorsChangesStats)
        {
            GitAuthorChangeStats[] authorsStatsOrdered = authorsChangesStats.SumByAuthor()
                .OrderByDescending(authorStats => authorStats.Insertions + authorStats.Deletions)
                .Where(stats => !stats.Author.Contains("Konrad J"))
                .ToArray()[..5];

            var rows = authorsStatsOrdered
                .Select((stats, i) => new object[]
                    { i + 1, stats.Author, stats.FilesChanged, stats.Insertions, stats.Deletions })
                .ToArray();

            return (headerRow: new[] { "Place", "Author", "Files changed", "Insertions", "Deletions" }, rows);
        }
    }
}