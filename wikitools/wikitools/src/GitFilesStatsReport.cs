using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record GitFilesStatsReport : MarkdownDocument
    {
        public static readonly object[] HeaderRow = { "Place", "FilePath", "Insertions", "Deletions" };
        public const string DescriptionFormat = "Git file changes since last {0} days as of {1}";

        public GitFilesStatsReport(
            ITimeline timeline,
            Task<GitLogCommit[]> commits,
            int days,
            int? top = null,
            Func<string, bool>? filePathFilter = null) : base(
            GetContent(timeline, days, commits, top, filePathFilter ?? (_ => true))) { }

        private static async Task<object[]> GetContent(
            ITimeline timeline,
            int days,
            Task<GitLogCommit[]> commits,
            int? top,
            Func<string, bool> filePathFilter) =>
            new object[]
            {
                string.Format(DescriptionFormat, days, timeline.UtcNow),
                "",
                new TabularData(GetRows(await commits, filePathFilter, top))
            };

        private static (object[] headerRow, object[][] rows) GetRows(
            GitLogCommit[] commits,
            Func<string, bool> filePathFilter,
            int? top)
        {
            var statsSumByFilePath = SumByFilePath(commits)
                .OrderByDescending(stats => stats.insertions + stats.deletions)
                .Where(stat => filePathFilter(stat.filePath))
                .ToArray();

            statsSumByFilePath = top is not null 
                ? statsSumByFilePath.Take((int) top).ToArray() 
                : statsSumByFilePath;

            var rows = statsSumByFilePath
                .Select((stats, i) => new object[] { i + 1, stats.filePath, stats.insertions, stats.deletions })
                .ToArray();

            return (HeaderRow, rows);
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