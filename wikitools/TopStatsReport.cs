using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

// kja TopStatsReport
// implement "top pages" report, which will show:
// - top 10 most edited pages last week. Might be less than 10 if not enough activity.
//   - Data will come from GitFilesStatsReport
// - top 10 most viewed pages last week. Might be less than 10 if not enough activity.
//   - Data will come from GitPagesViewsReport
// - Same as above, but for the last month.
// - Same as above, but for top 3 most active authors (with exclusions)
//   - Data already here (GitAuthorStats), but need to tweak filtering
// - Add annotations (icons): Newly added, lots of traffic (use :fire: in the MD)
//   - Emojis: https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops#emoji
public record TopStatsReport : MarkdownDocument
{
    public const string DescriptionFormat = "Git contributions since last {0} days as of {1}";

    public TopStatsReport(
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