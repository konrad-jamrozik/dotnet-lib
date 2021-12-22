using System.Threading.Tasks;
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

    private static async Task<object[]> GetContent(
        ITimeline timeline,
        GitLog gitLog,
        int top,
        int commitDays,
        string[]? excludedAuthors)
    {
        var stats = await GitAuthorStats.From(gitLog, commitDays, top, excludedAuthors);
        return new object[]
        {
            string.Format(DescriptionFormat, commitDays, timeline.UtcNow),
            "",
            GitAuthorStats.TabularData(stats)
        };
    }
}