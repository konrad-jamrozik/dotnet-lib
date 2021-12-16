using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record GitFilesStatsReport : MarkdownDocument
{
    // kj2 rename all "DescriptionFormat" to "PageHeader"
    public const string DescriptionFormat = "Git file changes since last {0} days as of {1}";

    public GitFilesStatsReport(
        ITimeline timeline,
        int days,
        GitFileStats[] dataRows) : base(
        GetContent(timeline, days, dataRows)) { }

    private static object[] GetContent(ITimeline timeline, int days, GitFileStats[] dataRows)
        =>
            new object[]
            {
                string.Format(DescriptionFormat, days, timeline.UtcNow),
                "",
                GitFileStats.TabularData(dataRows)
            };
}