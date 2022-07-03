using System.Threading.Tasks;
using Wikitools.Lib.Data;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record GitAuthorsStatsReport : MarkdownDocument
{
    public const string ReportHeaderFormatString =
        // kja this string should instead use GitLogCommits.DaySpan.ToString(), to be returned from
        // GitAuthorStats.From, called from GetContent
        "Git contributions since last {0} days as of {1}";

    public GitAuthorsStatsReport(
        ITimeline timeline,
        GitLog gitLog,
        int top,
        int commitDays,
        string[]? excludedAuthors = null)
        : base(GetContent(timeline, gitLog, top, commitDays, excludedAuthors)) { }

    private static async Task<object[]> GetContent(
        ITimeline timeline,
        GitLog gitLog,
        int top,
        int commitDays,
        string[]? excludedAuthors)
    {
        GitLogCommits commits = await gitLog.Commits(commitDays);
        RankedTop<GitAuthorStats> stats = GitAuthorStats.From(commits, top, excludedAuthors);
        return new object[]
        {
            string.Format(ReportHeaderFormatString, commitDays, timeline.UtcNow),
            "",
            GitAuthorStats.TabularData(stats)
        };
    }
}