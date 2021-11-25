using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

// kja TopStatsReport
// implement "top stats" report, which will show:
// - Top 5 most prolific authors in last 7 days - DONE
// - Top 10 most prolific authors in last 30 days - DONE

// - top 5 most edited pages last week
//   - Data already there (GitFileStats), but need to tweak filtering
// - top 5 most viewed pages last week.
//   - Data already here (GitPathViewStats), but need to tweak filtering
// - Same as above, but for the last month.
//   - Need to add it to report

// - Add annotations (icons): Newly added, lots of traffic (use :fire: in the MD)
//   - Emojis: https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops#emoji
public record TopStatsReport : MarkdownDocument
{
    public const string AuthorDescriptionFormat = "Top {0} git contributions since last {1} days as of {2}";

    public const string FileDescriptionFormat = "Top {0} Git files by insertions since last {1} days as of {2}";

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
        GitAuthorStats[] authorDataRowsLastWeek,
        GitAuthorStats[] authorDataRowsLast30Days,
        GitFileStats[] fileDataRows,
        PathViewStats[] pathViewDataRows) : base(
        GetContent(
            timeline,
            days,
            pageViewsForDays,
            authorDataRowsLastWeek,
            authorDataRowsLast30Days,
            fileDataRows,
            pathViewDataRows)) { }

    private static object[] GetContent(
        ITimeline timeline,
        int days,
        int pageViewsForDays,
        GitAuthorStats[] authorDataRowsLastWeek,
        GitAuthorStats[] authorDataRowsLast30Days,
        GitFileStats[] fileDataRows,
        PathViewStats[] pathViewDataRows)
        =>
            new object[]
            {
                // kj2 dehardcode magic constants
                string.Format(AuthorDescriptionFormat, 5, 7, new DateDay(timeline.UtcNow).AddDays(-1)),
                "",
                GitAuthorStats.TabularData(authorDataRowsLastWeek),
                "",
                string.Format(AuthorDescriptionFormat, 10, 30, new DateDay(timeline.UtcNow).AddDays(-1)),
                "",
                GitAuthorStats.TabularData(authorDataRowsLast30Days),
                "",
                string.Format(FileDescriptionFormat, 10, 30, timeline.UtcNow),
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