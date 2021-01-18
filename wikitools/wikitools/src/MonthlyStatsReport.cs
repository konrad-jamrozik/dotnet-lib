using System;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record MonthlyStatsReport : MarkdownDocument
    {
        public MonthlyStatsReport(
            GitLogCommit[] commits,
            Func<string, bool>? authorFilter = null,
            Func<string, bool>? filePathFilter = null) : base(
            GetContent(commits,
                authorFilter ?? (_ => true),
                filePathFilter ?? (_ => true))) { }

        private static object[] GetContent(
            GitLogCommit[] commits,
            Func<string, bool> authorFilter,
            Func<string, bool> filePathFilter) =>
            new object[]
            {
                $"Git file insertions and deletions month over month",
                "",
                new TabularData(GetRows(commits, authorFilter, filePathFilter))
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

            var insertionsByMonth = commitsByMonth.Select(mcs => (
                    month: mcs.Key,
                    insertions: mcs.Sum(c => c.Stats.Where(FilePathFilter).Sum(ns => ns.Insertions)),
                    deletions: mcs.Sum(c => c.Stats.Where(FilePathFilter).Sum(ns => ns.Deletions))
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