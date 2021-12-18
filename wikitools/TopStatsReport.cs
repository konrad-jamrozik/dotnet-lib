using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record TopStatsReport : MarkdownDocument
{
    private const string AuthorDescriptionFormat = "# Top {0} contributors since {1} days";

    private const string FileDescriptionFormat = "# Top {0} files by insertions since {1} days";

    private const string PageViewDescriptionFormat = "# Top {0} pages by views since {1} days";

    public TopStatsReport(
        Timeline timeline,
        GitLog gitLog,
        IAdoWiki wiki,
        string[]? excludedAuthors,
        string[]? excludedPaths)
        : base(GetContent(timeline, gitLog, wiki, excludedAuthors, excludedPaths)) { }


    private static object[] GetContent(
        Timeline timeline,
        GitLog gitLog,
        IAdoWiki wiki,
        string[]? excludedAuthors,
        string[]? excludedPaths)
    {
        var top3 = 3;
        var top5 = 5;
        var top10 = 10;
        var days7 = 7;
        var days28 = 28;

        // kj2 currently going up to 8 days back, and including today, which is partial.
        // Should instead be exactly from 8 days back (inclusive) to 1 day back (inclusive),
        // but without today.
        // Need same fix for git file stats. But page stats are already solved.
        var authorStatsLast7Days = GitAuthorStats.From(gitLog, days7, excludedAuthors, top3);
        var authorStatsLast28Days = GitAuthorStats.From(gitLog, days28, excludedAuthors, top5);
        var fileStatsLast7Days = GitFileStats.From(gitLog, days7, excludedPaths, top5);
        var fileStatsLast28Days = GitFileStats.From(gitLog, days28, excludedPaths, top10);

        var ago1Day = timeline.DaysFromUtcNow(-1);
        var ago7Days = timeline.DaysFromUtcNow(-7);
        var ago28Days = timeline.DaysFromUtcNow(-28);
        var last7Days = new DaySpan(ago7Days, ago1Day);
        var last28Days = new DaySpan(ago28Days, ago1Day);

        var pagesStatsLast7Days = PageViewStats.From(timeline, wiki, last7Days, top5);
        var pagesStatsLast28Days = PageViewStats.From(timeline, wiki, last28Days, top10);


        return new object[]
        {
            $"This page was generated on {timeline.UtcNow} UTC",
            $"All day ranges are up until EOD {timeline.DaysFromUtcNow(-1)} UTC",
            "",
            "[[_TOC_]]",
            "",
            string.Format(AuthorDescriptionFormat, authorStatsLast7Days.Top, days7),
            "",
            // kj would be cool if paths in these tables are hyperlinked, like in WTOC.
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
            "",
            string.Format(PageViewDescriptionFormat, pagesStatsLast7Days.Top, days7),
            "",
            PageViewStats.TabularData(pagesStatsLast7Days),
            "",
            string.Format(PageViewDescriptionFormat, pagesStatsLast28Days.Top, days28),
            "",
            PageViewStats.TabularData(pagesStatsLast28Days)
        };
    }
}