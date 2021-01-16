using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public class GitAuthorsStatsReport2 : MarkdownDocument
    {
        private readonly Timeline _timeline;
        private readonly int _days;
        private readonly List<GitAuthorChangeStats> _stats;

        public GitAuthorsStatsReport2(Timeline timeline, int days, List<GitAuthorChangeStats> stats)
        {
            _timeline = timeline;
            _days = days;
            _stats = stats;
        }

        protected override List<object> Content =>
            new()
            {
                $"Git contributions since last {_days} days as of {_timeline.UtcNow}",
                "",
                new TabularData2(GetAuthorsChangesStatsRows(_stats))
            };

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