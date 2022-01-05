using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Data;
using Wikitools.Lib.Primitives;

namespace Wikitools;

// kj2 maybe instead of inheriting MarkdownDocument, it should be an interface, like:
// IMarkdownDocumentConvertible with method .ToMarkdownDocument.
// Or instead introduce a new MarkdownDocument ctor taking this class as param?
// This would require access to internals, perhaps.
public record MonthlyStatsReport : MarkdownDocument
{
    public MonthlyStatsReport(
        GitLog gitLog,
        DaySpan logsDaySpan,
        string[] excludedAuthors,
        string[] excludedPaths) : base(
        GetContent(
            gitLog,
            logsDaySpan,
            excludedAuthors,
            excludedPaths)) { }

    private static async Task<object[]> GetContent(
        GitLog gitLog,
        DaySpan logsDaySpan,
        string[] excludedAuthors,
        string[] excludedPaths) =>
        new object[]
        {
            "Git file insertions and deletions month over month",
            "",
            new TabularData(await GetRows(gitLog, logsDaySpan, excludedAuthors, excludedPaths))
        };

    private static async Task<(object[] headerRow, object[][] rows)> GetRows(
        GitLog gitLog,
        DaySpan logsDaySpan,
        string[] excludedAuthors,
        string[] excludedPaths)
    {
        var commits = await gitLog.Commits(logsDaySpan);

        // kja dedup all filter logic into .WhereNotContains()
        Func<string, bool> authorFilter = excludedAuthors != null
            ? author => !excludedAuthors.Any(author.Contains)
            : _ => true;

        Func<string, bool> filePathFilter = excludedPaths != null
            ? path => !excludedPaths.Any(path.Contains)
            : _ => true;

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