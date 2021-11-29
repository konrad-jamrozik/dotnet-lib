using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record TopStatsReport : MarkdownDocument
{
    private const string AuthorDescriptionFormat =
        "# Top {0} contributors since {1} days";

    private const string FileDescriptionFormat =
        "# Top {0} files by insertions since {1} days";

    private const string PageViewDescriptionFormat =
        "# Top {0} pages by views since {1} days";

    // kja the report itself decides what are the top ranges (7d, 30d, top 10), so it needs to
    // to do the filtering itself, and not get the data as input.
    // UNLESS the data should come from config, like "TopStatsReportSettings" json element.
    // Then this report just unpacks a bundle of stats, like "TopStats", which has "TopStats.AuthorStatsForLastWeek".
    // Yeah, I think I will do the "TopStats as input" approach, but perhaps the 7d/30d/top 10 won't 
    // necessarily come from config; it will be hardcoded in the TopStats type itself.
    public TopStatsReport(
        Timeline timeline,
        GitAuthorStats[] authorDataRowsLast7Days,
        GitAuthorStats[] authorDataRowsLast28Days,
        GitFileStats[] fileDataRowsLast7Days,
        GitFileStats[] fileDataRowsLast28Days,
        PageViewStats[] pageViewDataRowsLast7Days,
        PageViewStats[] pageViewDataRowsLast28Days) : base(
        GetContent(
            timeline,
            authorDataRowsLast7Days,
            authorDataRowsLast28Days,
            fileDataRowsLast7Days,
            fileDataRowsLast28Days,
            pageViewDataRowsLast7Days,
            pageViewDataRowsLast28Days)) { }

    private static object[] GetContent(
        Timeline timeline,
        GitAuthorStats[] authorDataRowsLast7Days,
        GitAuthorStats[] authorDataRowsLast30Days,
        GitFileStats[] fileDataRowsLast7Days,
        GitFileStats[] fileDataRowsLast30Days,
        PageViewStats[] pageViewDataRowsLast7Days,
        PageViewStats[] pageViewDataRowsLast28Days)
        =>
            new object[]
            {
                $"This page was generated on {timeline.UtcNow} UTC",
                $"All day ranges are up until EOD {timeline.DaysFromUtcNow(-1)} UTC",
                "",
                "[[_TOC_]]",
                "",
                // kj2 dehardcode magic constants. They should come from the stats collections themselves: both ranges and tops.
                string.Format(AuthorDescriptionFormat, 3, 7),
                "",
                // kj2 would be cool if paths in these tables are hyperlinked, like in WTOC.
                GitAuthorStats.TabularData(authorDataRowsLast7Days), 
                "",
                string.Format(AuthorDescriptionFormat, 5, 28),
                "",
                GitAuthorStats.TabularData(authorDataRowsLast30Days),
                "",
                string.Format(FileDescriptionFormat, 5, 7),
                "",
                GitFileStats.TabularData(fileDataRowsLast7Days),
                "",
                string.Format(FileDescriptionFormat, 10, 28),
                "",
                GitFileStats.TabularData(fileDataRowsLast30Days),
                "",
                string.Format(PageViewDescriptionFormat, 5, 7),
                "",
                PageViewStats.TabularData(pageViewDataRowsLast7Days),
                "",
                string.Format(PageViewDescriptionFormat, 10, 28),
                "",
                PageViewStats.TabularData(pageViewDataRowsLast28Days)
            };
}