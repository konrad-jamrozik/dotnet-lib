using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record GitFilesStatsReport2 : MarkdownDocument
    {
        public GitFilesStatsReport2(
            ITimeline timeline,
            int days,
            GitLogCommit[] commits,
            Func<string, bool> filePathFilter) : base(
            GetContent(timeline, days, commits, filePathFilter)) { }

        private static List<object> GetContent(
            ITimeline timeline,
            int days,
            GitLogCommit[] commits,
            Func<string, bool> filePathFilter) =>
            new()
            {
                $"Git file changes since last {days} days as of {timeline.UtcNow}",
                "",
                new TabularData2(GetRows(commits, filePathFilter))
            };

        private static (string[] headerRow, object[][] rows) GetRows(
            GitLogCommit[] commits,
            Func<string, bool> filePathFilter)
        {
            var statsSumByFilePath = SumByFilePath(commits)
                .OrderByDescending(stats => stats.insertions + stats.deletions)
                .Where(stat => filePathFilter(stat.filePath))
                .ToArray()
                .Take(5);

            var rows = statsSumByFilePath
                .Select((stats, i) => new object[] { i + 1, stats.filePath, stats.insertions, stats.deletions })
                .ToArray();

            return (headerRow: new[] { "Place", "FilePath", "Insertions", "Deletions" }, rows);
        }

        private static (string filePath, int insertions, int deletions)[] SumByFilePath(GitLogCommit[] commits)
        {
            var fileStats       = commits.SelectMany(c => c.Stats.Select(s => (s.FilePath, s.Insertions, s.Deletions)));
            var statsByFilePath = fileStats.GroupBy(s => s.FilePath);
            var statsSumByFilePath = statsByFilePath.Select(pathStats =>
                (
                    filePath: pathStats.Key,
                    insertions: pathStats.Sum(s => s.Insertions),
                    deletions: pathStats.Sum(s => s.Deletions)
                )
            );
            return statsSumByFilePath.ToArray();
        }
    }
}