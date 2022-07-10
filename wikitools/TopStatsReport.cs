using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record TopStatsReport : MarkdownDocument
{
    private const string AuthorDescriptionFormat = "# Top {0} contributors for {1}";

    private const string FileDescriptionFormat = "# Top {0} files by insertions for {1}";

    private const string PageViewDescriptionFormat = "# Top {0} pages by views for {1}";

    public TopStatsReport(
        Timeline timeline,
        GitLog gitLog,
        IAdoWiki wiki,
        string[]? excludedAuthors,
        string[]? excludedPaths)
        : base(GetContent(timeline, gitLog, wiki, excludedAuthors, excludedPaths)) { }


    private static async Task<object[]> GetContent(
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

        GitLogCommits commits7Days = await gitLog.Commits(days7);
        GitLogCommits commits28Days = await gitLog.Commits(days28);
        var authorStatsLast7Days = GitAuthorStats.From(commits7Days, top3, excludedAuthors, excludedPaths);
        var authorStatsLast28Days = GitAuthorStats.From(commits28Days, top5, excludedAuthors, excludedPaths);
        var fileStatsLast7Days = GitFileStats.From(commits7Days, top5, excludedPaths);
        var fileStatsLast28Days = GitFileStats.From(commits28Days, top10, excludedPaths);

        var ago1Day = timeline.DaysFromUtcNow(-1);
        var ago7Days = timeline.DaysFromUtcNow(-7);
        var ago28Days = timeline.DaysFromUtcNow(-28);
        var last7Days = new DaySpan(ago7Days, ago1Day);
        var last28Days = new DaySpan(ago28Days, ago1Day);

        var pagesStatsLast7Days = await PageViewStats.From(timeline, wiki, last7Days, top5);
        var pagesStatsLast28Days = await PageViewStats.From(timeline, wiki, last28Days, top10);

        var daySpanHeaderFor7Days = commits7Days.DaySpan.ToPrettyString();
        var daySpanHeaderFor28Days = commits28Days.DaySpan.ToPrettyString();
        return new object[]
        {
            $"This page was generated on {timeline.UtcNow.Format(includeTime: true, includeTimezone: true)}",
            "",
            "[[_TOC_]]",
            "",
            string.Format(AuthorDescriptionFormat, authorStatsLast7Days.Top, daySpanHeaderFor7Days),
            "",
            GitAuthorStats.TabularData(authorStatsLast7Days),
            "",
            string.Format(AuthorDescriptionFormat, authorStatsLast28Days.Top, daySpanHeaderFor28Days),
            "",
            GitAuthorStats.TabularData(authorStatsLast28Days),
            "",
            string.Format(FileDescriptionFormat, fileStatsLast7Days.Top, daySpanHeaderFor7Days),
            "",
            GitFileStats.TabularData(fileStatsLast7Days),
            "",
            string.Format(FileDescriptionFormat, fileStatsLast28Days.Top, daySpanHeaderFor28Days),
            "",
            GitFileStats.TabularData(fileStatsLast28Days),
            "",
            string.Format(PageViewDescriptionFormat, pagesStatsLast7Days.Top, daySpanHeaderFor7Days),
            "",
            PageViewStats.TabularData(pagesStatsLast7Days),
            "",
            string.Format(PageViewDescriptionFormat, pagesStatsLast28Days.Top, daySpanHeaderFor28Days),
            "",
            PageViewStats.TabularData(pagesStatsLast28Days)
        };
    }
}