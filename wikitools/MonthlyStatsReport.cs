using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Data;

namespace Wikitools;

// kj2 maybe instead of inheriting MarkdownDocument, it should be an interface, like:
// IMarkdownDocumentConvertible with method .ToMarkdownDocument.
// Or instead introduce a new MarkdownDocument ctor taking this class as param?
// This would require access to internals, perhaps.
public record MonthlyStatsReport : MarkdownDocument
{
    public MonthlyStatsReport(
        Task<GitLogCommits> commits,
        Func<string, bool>? authorFilter = null,
        Func<string, bool>? filePathFilter = null) : base(
        GetContent(commits,
            authorFilter ?? (_ => true),
            filePathFilter ?? (_ => true))) { }

    private static async Task<object[]> GetContent(
        Task<GitLogCommits> commits,
        Func<string, bool> authorFilter,
        Func<string, bool> filePathFilter) =>
        new object[]
        {
            "Git file insertions and deletions month over month",
            "",
            new TabularData(GetRows(await commits, authorFilter, filePathFilter))
        };

    private static (object[] headerRow, object[][] rows) GetRows(
        GitLogCommits commits, // kja pass GitLog instead. See TopStatsReport
        Func<string, bool> authorFilter,
        Func<string, bool> filePathFilter)
    {
        var commitsByMonth = commits
            .Where(commit => authorFilter(commit.Author))
            .GroupBy(commit => $"{commit.Date.Year} {commit.Date.Month}");

        bool FilePathFilter(GitLogCommit.Numstat stat) => filePathFilter(stat.Path.ToString());

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