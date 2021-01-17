using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public class GitAuthorsStatsReport2 : MarkdownDocument
    {
        private readonly ITimeline _timeline;
        private readonly int _days;
        private readonly List<GitAuthorChangeStats> _stats;

        public GitAuthorsStatsReport2(ITimeline timeline, int days, List<GitAuthorChangeStats> stats)
        {
            _timeline = timeline;
            _days = days;
            _stats = stats;
        }

        public override List<object> Content =>
            new()
            {
                $"Git contributions since last {_days} days as of {_timeline.UtcNow}",
                "",
                new TabularData2(Rows(_stats))
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