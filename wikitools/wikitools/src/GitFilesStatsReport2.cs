using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record GitFilesStatsReport2(ITimeline Timeline, int Days, List<GitFileChangeStats> Stats) : MarkdownDocument
    {
        public override List<object> Content =>
            new()
            {
                $"Git file changes since last {Days} days as of {Timeline.UtcNow}",
                "",
                new TabularData2(GetRows(Stats))
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