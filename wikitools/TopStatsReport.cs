using Wikitools.Lib.Data;
using Wikitools.Lib.Git;
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
    public TopStatsReport(
        Timeline timeline,
        RankedTop<GitAuthorStats> authorDataRowsLast7Days,
        RankedTop<GitAuthorStats> authorDataRowsLast28Days,
        RankedTop<GitFileStats> fileDataRowsLast7Days,
        RankedTop<GitFileStats> fileDataRowsLast28Days,
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

    // kja migrate the integration test to this ctor. Then gradually fill it up.
    public TopStatsReport(
        Timeline timeline,
        GitLog gitLog,
        string[]? excludedAuthors,
        string[]? excludedPaths) 
        : base(GetContent2(timeline, gitLog, excludedAuthors, excludedPaths)) {}


    private static object[] GetContent2(Timeline timeline, GitLog gitLog, string[]? excludedAuthors, string[]? excludedPaths)
    {
        var top3 = 3;
        var top5 = 5;
        var top10 = 10;
        var days7 = 7;
        var days28 = 28;
        var authorStatsLast7Days = GitAuthorStats.From2(gitLog, days7, excludedAuthors, top3);
        var authorStatsLast28Days = GitAuthorStats.From2(gitLog, days28, excludedAuthors, top5);
        var fileStatsLast7Days = GitFileStats.From2(gitLog, days7, excludedPaths, top5);
        var fileStatsLast28Days = GitFileStats.From2(gitLog, days28, excludedPaths, top10);

        return new object[]
        {
            $"This page was generated on {timeline.UtcNow} UTC",
            $"All day ranges are up until EOD {timeline.DaysFromUtcNow(-1)} UTC",
            "",
            "[[_TOC_]]",
            "",
            string.Format(AuthorDescriptionFormat, authorStatsLast7Days.Top, days7),
            "",
            // kj2 would be cool if paths in these tables are hyperlinked, like in WTOC.
            GitAuthorStats.TabularData(authorStatsLast7Days),
            "",
            string.Format(AuthorDescriptionFormat, authorStatsLast28Days.Top, days28),
            "",
            GitAuthorStats.TabularData(authorStatsLast28Days),
            "",
            string.Format(FileDescriptionFormat, fileStatsLast7Days.Top, days7),
            "",
            GitFileStats.TabularData(fileStatsLast7Days),
            "",
            string.Format(FileDescriptionFormat, fileStatsLast28Days.Top, days28),
            "",
            GitFileStats.TabularData(fileStatsLast28Days),
        };
    }

    private static object[] GetContent(
        Timeline timeline,
        RankedTop<GitAuthorStats> authorDataRowsLast7Days,
        RankedTop<GitAuthorStats> authorDataRowsLast28Days,
        RankedTop<GitFileStats> fileDataRowsLast7Days,
        RankedTop<GitFileStats> fileDataRowsLast28Days,
        PageViewStats[] pageViewDataRowsLast7Days,
        PageViewStats[] pageViewDataRowsLast28Days)
    {
        return new object[]
        {
            $"This page was generated on {timeline.UtcNow} UTC",
            $"All day ranges are up until EOD {timeline.DaysFromUtcNow(-1)} UTC",
            "",
            "[[_TOC_]]",
            "",
            // kj2 dehardcode magic constants. They should come from the stats collections themselves: both ranges and tops.
            string.Format(AuthorDescriptionFormat, authorDataRowsLast7Days.Top, 7),
            "",
            // kj2 would be cool if paths in these tables are hyperlinked, like in WTOC.
            GitAuthorStats.TabularData(authorDataRowsLast7Days),
            "",
            string.Format(AuthorDescriptionFormat, authorDataRowsLast28Days.Top, 28),
            "",
            GitAuthorStats.TabularData(authorDataRowsLast28Days),
            "",
            string.Format(FileDescriptionFormat, 5, 7),
            "",
            GitFileStats.TabularData(fileDataRowsLast7Days),
            "",
            string.Format(FileDescriptionFormat, 10, 28),
            "",
            GitFileStats.TabularData(fileDataRowsLast28Days),
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
}