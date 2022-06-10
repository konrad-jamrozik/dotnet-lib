using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Data;
using Wikitools.Lib.Primitives;

namespace Wikitools;

// kj2-report maybe instead of inheriting MarkdownDocument, it should be an interface, like:
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

        var commitsByMonth = commits
            .WhereNotContains(commit => commit.Author, excludedAuthors)
            .GroupBy(commit => $"{commit.Date.Year} {commit.Date.Month}");


        var operationsByMonth = commitsByMonth.Select(mcs => (
                month: mcs.Key,
                insertions: mcs.Sum(
                    c => c.Stats.WhereNotContains(stat => stat.Path.ToString(), excludedPaths)
                        .Sum(ns => ns.Insertions)),
                deletions: mcs.Sum(
                    c => c.Stats.WhereNotContains(stat => stat.Path.ToString(), excludedPaths)
                        .Sum(ns => ns.Deletions))
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