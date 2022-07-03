using System.Threading.Tasks;
using Wikitools.Lib.Data;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;

namespace Wikitools;

public record GitAuthorsStatsReport : MarkdownDocument
{
    public const string ReportHeaderFormatString =
        "Git contributions for {0}, both inclusive.";

    public GitAuthorsStatsReport(
        GitLog gitLog,
        int top,
        int commitDays,
        string[]? excludedAuthors = null)
        : base(GetContent(gitLog, top, commitDays, excludedAuthors)) { }

    private static async Task<object[]> GetContent(
        GitLog gitLog,
        int top,
        int commitDays,
        string[]? excludedAuthors)
    {
        GitLogCommits commits = await gitLog.Commits(commitDays);
        RankedTop<GitAuthorStats> stats = GitAuthorStats.From(commits, top, excludedAuthors);
        return new object[]
        {
            string.Format(ReportHeaderFormatString, commits.DaySpan.ToPrettyString()),
            "",
            GitAuthorStats.TabularData(stats)
        };
    }
}