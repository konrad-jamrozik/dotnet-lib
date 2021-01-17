using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record MonthlyStatsReport(ITimeline Timeline, GitLogCommit[] Commits) : MarkdownDocument
    {
        public override List<object> Content =>
            new()
            {
                $"Git file insertions and deletions month over month",
                "",
                new TabularData2(GetRows(Commits))
            };

        private static (object[] headerRow, object[][] rows) GetRows(GitLogCommit[] commits)
        {
            var commitsByMonth = commits
                .Where(commit => !commit.Author.Contains("Konrad J"))
                .GroupBy(commit => $"{commit.Date.Year} {commit.Date.Month}");

            static bool FilePathFilter(GitLogCommit.Numstat stat) => !stat.FilePath.Contains("/Meta");

            var insertionsByMonth = commitsByMonth.Select(month => (
                    month: month.Key,
                    insertions: month.Sum(c => c.Stats.Where(FilePathFilter).Sum(ns => ns.Insertions)),
                    deletions: month.Sum(c => c.Stats.Where(FilePathFilter).Sum(ns => ns.Deletions))
                )
            );

            var rows = insertionsByMonth
                .Select(monthData => new object[]
                {
                    monthData.month,
                    monthData.insertions,
                    monthData.deletions
                })
                .ToArray();

            return (headerRow: new object[] { "Month", "Insertions", "Deletions" }, rows);
        }
    }
}