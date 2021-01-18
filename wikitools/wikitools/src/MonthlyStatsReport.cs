using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record MonthlyStatsReport : MarkdownDocument
    {
        public MonthlyStatsReport(
            Task<GitLogCommit[]> commits,
            Func<string, bool>? authorFilter = null,
            Func<string, bool>? filePathFilter = null) : base(
            GetContent(commits,
                authorFilter ?? (_ => true),
                filePathFilter ?? (_ => true))) { }

        private static async Task<object[]> GetContent(
            Task<GitLogCommit[]> commits,
            Func<string, bool> authorFilter,
            Func<string, bool> filePathFilter) =>
            new object[]
            {
                $"Git file insertions and deletions month over month",
                "",
                new TabularData(GetRows(await commits, authorFilter, filePathFilter))
            };

        private static (object[] headerRow, object[][] rows) GetRows(
            GitLogCommit[] commits,
            Func<string, bool> authorFilter,
            Func<string, bool> filePathFilter)
        {
            var commitsByMonth = commits
                .Where(commit => authorFilter(commit.Author))
                .GroupBy(commit => $"{commit.Date.Year} {commit.Date.Month}");

            bool FilePathFilter(GitLogCommit.Numstat stat) => filePathFilter(stat.FilePath);

            var operationsByMonth = commitsByMonth.Select(mcs => (
                    month: mcs.Key,
                    insertions: mcs.Sum(c => c.Stats.Where(FilePathFilter).Sum(ns => ns.Insertions)),
                    deletions: mcs.Sum(c => c.Stats.Where(FilePathFilter).Sum(ns => ns.Deletions))
                )
            );

            var rows = operationsByMonth
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