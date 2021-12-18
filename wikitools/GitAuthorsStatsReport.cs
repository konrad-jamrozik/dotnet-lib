using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record GitAuthorsStatsReport : MarkdownDocument
{
    public const string DescriptionFormat = "Git contributions since last {0} days as of {1}";

    public GitAuthorsStatsReport(
        ITimeline timeline,
        GitLog gitLog,
        int top,
        int commitDays,
        string[]? excludedAuthors = null)
        : base(GetContent(timeline, gitLog, top, commitDays, excludedAuthors)) { }

    private static object[] GetContent(
        ITimeline timeline,
        GitLog gitLog,
        int top,
        int commitDays,
        string[]? excludedAuthors)
    {
        var stats = GitAuthorStats.From(gitLog, commitDays, excludedAuthors, top);
        return new object[]
        {
            string.Format(DescriptionFormat, commitDays, timeline.UtcNow),
            "",
            GitAuthorStats.TabularData(stats)
        };
    }
}