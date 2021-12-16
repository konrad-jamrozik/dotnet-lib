using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record GitAuthorsStatsReport : MarkdownDocument
{
    public const string DescriptionFormat = "Git contributions since last {0} days as of {1}";

    public GitAuthorsStatsReport(
        ITimeline timeline,
        int days,
        GitAuthorStats[] dataRows) : base(
        GetContent(timeline, days, dataRows)) { }

    private static object[] GetContent(ITimeline timeline, int days, GitAuthorStats[] dataRows)
        =>
            new object[]
            {
                string.Format(DescriptionFormat, days, timeline.UtcNow),
                "",
                GitAuthorStats.TabularData(dataRows)
            };
}