using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

// kja TopStatsReport
// implement "top stats" report, which will show:
// - Top 5 most prolific authors in last 7 days - DONE
// - Top 10 most prolific authors in last 30 days - DONE
// - top 10 most edited pages last 7 days - DONE
// - top 20 most edited pages last 30 days - DONE
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
        "Top {0} paths by views since last {1} days as of {2}";

    // kja the report itself decides what are the top ranges (7d, 30d, top 10), so it needs to
    // to do the filtering itself, and not get the data as input.
    // UNLESS the data should come from config, like "TopStatsReportSettings" json element.
    // Then this report just unpacks a bundle of stats, like "TopStats", which has "TopStats.AuthorStatsForLastWeek".
    // Yeah, I think I will do the "TopStats as input" approach, but perhaps the 7d/30d/top 10 won't 
    // necessarily come from config; it will be hardcoded in the TopStats type itself.
    public TopStatsReport(
        Timeline timeline,
        int pageViewsForDays,
        GitAuthorStats[] authorDataRowsLast7Days,
        GitAuthorStats[] authorDataRowsLast30Days,
        GitFileStats[] fileDataRowsLast7Days,
        GitFileStats[] fileDataRowsLast30Days,
        PathViewStats[] pathViewDataRows) : base(
        GetContent(
            timeline,
            pageViewsForDays,
            authorDataRowsLast7Days,
            authorDataRowsLast30Days,
            fileDataRowsLast7Days,
            fileDataRowsLast30Days,
            pathViewDataRows)) { }

    private static object[] GetContent(
        Timeline timeline,
        int pageViewsForDays,
        GitAuthorStats[] authorDataRowsLast7Days,
        GitAuthorStats[] authorDataRowsLast30Days,
        GitFileStats[] fileDataRowsLast7Days,
        GitFileStats[] fileDataRowsLast30Days,
        PathViewStats[] pathViewDataRows)
        =>
            new object[]
            {
                // kj2 dehardcode magic constants. They should come from the stats collections themselves: both ranges and tops.
                string.Format(AuthorDescriptionFormat, 5, 7, timeline.DaysFromUtcNow(-1)),
                "",
                GitAuthorStats.TabularData(authorDataRowsLast7Days),
                "",
                string.Format(AuthorDescriptionFormat, 10, 30, timeline.DaysFromUtcNow(-1)),
                "",
                GitAuthorStats.TabularData(authorDataRowsLast30Days),
                "",
                string.Format(FileDescriptionFormat, 10, 7, timeline.DaysFromUtcNow(-1)),
                "",
                GitFileStats.TabularData(fileDataRowsLast7Days),
                "",
                string.Format(FileDescriptionFormat, 20, 30, timeline.DaysFromUtcNow(-1)),
                "",
                GitFileStats.TabularData(fileDataRowsLast30Days),
                "",
                string.Format(PathViewDescriptionFormat, 10, 7, timeline.DaysFromUtcNow(-1)),
                "",
                PathViewStats.TabularData(pathViewDataRows)
            };
}