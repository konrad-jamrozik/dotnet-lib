using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public class GitFilesStatsReport2 : MarkdownDocument
    {
        private readonly ITimeline _timeline;
        private readonly int _days;
        private readonly List<GitFileChangeStats> _stats;

        public GitFilesStatsReport2(ITimeline timeline, int days, List<GitFileChangeStats> stats)
        {
            _timeline = timeline;
            _days = days;
            _stats = stats;
        }

        public override List<object> Content =>
            new()
            {
                $"Git file changes since last {_days} days as of {_timeline.UtcNow}",
                "",
                new TabularData2(GetRows(_stats))
            };

        private static (string[] headerRow, object[][] rows) GetRows(
            List<GitFileChangeStats> filesChangesStats)
        {
            GitFileChangeStats[] filesStatsOrdered = filesChangesStats.SumByFilePath()
                .OrderByDescending(fileStats => fileStats.Insertions + fileStats.Deletions)
                .Where(row => !row.FilePath.Contains("/Meta"))
                .ToArray()[..5];

            var rows = filesStatsOrdered
                .Select((stats, i) => new object[] { i + 1, stats.FilePath, stats.Insertions, stats.Deletions })
                .ToArray();

            return (headerRow: new[] { "Place", "FilePath", "Insertions", "Deletions" }, rows);
        }
    }
}