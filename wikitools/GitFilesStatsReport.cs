using Wikitools.Lib.Data;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record GitFilesStatsReport : MarkdownDocument
{
    public const string ReportHeaderFormatString = "Git file changes since last {0} days as of {1}";

    public GitFilesStatsReport(
        ITimeline timeline,
        int days,
        RankedTop<GitFileStats> dataRows) : base(
        GetContent(timeline, days, dataRows)) { }

    private static object[] GetContent(
        ITimeline timeline,
        int days,
        RankedTop<GitFileStats> dataRows)
        =>
            new object[]
            {
                string.Format(ReportHeaderFormatString, days, timeline.UtcNow),
                "",
                GitFileStats.TabularData(dataRows)
            };
}