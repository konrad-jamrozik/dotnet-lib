using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

// kja TopStatsReport
// implement "top stats" report, which will show:
// - top 10 most edited pages last week. Might be less than 10 if not enough activity.
//   - Data already there (GitFileStats), but need to tweak filtering
// - top 10 most viewed pages last week. Might be less than 10 if not enough activity.
//   - Data already here (GitPathViewStats), but need to tweak filtering
// - Same as above, but for the last month.
//   - Need to add it to report
// - Same as above, but for top 3 most active authors (with exclusions)
//   - Data already here (GitAuthorStats), but need to tweak filtering
// - Add annotations (icons): Newly added, lots of traffic (use :fire: in the MD)
//   - Emojis: https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops#emoji
public record TopStatsReport : MarkdownDocument
{
    public const string AuthorDescriptionFormat = "Git contributions since last {0} days as of {1}";

    public const string FileDescriptionFormat = "Git file changes since last {0} days as of {1}";

    public const string PathViewDescriptionFormat =
        "Path views since last {0} days as of {1}. Total wiki pages: {2}";

    // kja the report itself decides what are the top ranges (7d, 30d, top 10), so it needs to
    // to do the filtering itself, and not get the data as input.
    // UNLESS the data should come from config, like "TopStatsReportSettings" json element.
    // Then this report just unpacks a bundle of stats, like "TopStats", which has "TopStats.AuthorStatsForLastWeek".
    // Yeah, I think I will do the "TopStats as input" approach, but perhaps the 7d/30d/top 10 won't 
    // necessarily come from config; it will be hardcoded in the TopStats type itself.
    public TopStatsReport(
        ITimeline timeline,
        int days,
        int pageViewsForDays,
        GitAuthorStats[] authorDataRows,
        GitFileStats[] fileDataRows,
        PathViewStats[] pathViewDataRows) : base(
        GetContent(timeline, days, pageViewsForDays, authorDataRows, fileDataRows, pathViewDataRows)) { }

    private static object[] GetContent(
        ITimeline timeline,
        int days,
        int pageViewsForDays,
        GitAuthorStats[] authorDataRows,
        GitFileStats[] fileDataRows,
        PathViewStats[] pathViewDataRows)
        =>
            new object[]
            {
                string.Format(AuthorDescriptionFormat, days, timeline.UtcNow),
                "",
                GitAuthorStats.TabularData(authorDataRows),
                "",
                string.Format(FileDescriptionFormat, days, timeline.UtcNow),
                "",
                GitFileStats.TabularData(fileDataRows),
                "",
                string.Format(
                    PathViewDescriptionFormat,
                    pageViewsForDays,
                    timeline.UtcNow,
                    pathViewDataRows.Length),
                "",
                PathViewStats.TabularData(pathViewDataRows)
            };
}