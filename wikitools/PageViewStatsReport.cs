using Wikitools.AzureDevOps;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record PageViewStatsReport : MarkdownDocument
{
    public const string DescriptionFormat =
        "Page views since last {0} days as of {1}. Total wiki pages: {2}";

    public PageViewStatsReport(
        ITimeline timeline,
        IAdoWiki wiki,
        int pageViewsForDays) : base(
        GetContent(timeline, wiki, pageViewsForDays)) { }

    private static object[] GetContent(
        ITimeline timeline,
        IAdoWiki wiki,
        int pageViewsForDays)
    {
        // kj2 this will trigger call to ADO API.
        // Here is is OK, as we are in late execution stage, but I need to ensure
        // this is fixed everywhere, always deferred to the execution stage.
        // Below are previous ideas I had, now obsolete:
        // I might need to fix all Tasks to AsyncLazy to make this work, or by using new Task() and then task.Start();
        // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-5.0#separating-task-creation-and-execution
        // Maybe source generators could help here. See [Cache] and [Memoize] use cases here:
        // https://github.com/dotnet/roslyn/issues/16160
        // 11/17/2021: Or maybe doing stuff like LINQ IEnumerable is enough? IEnumerable and related
        // collections are lazy after all.
        var wikiStats = wiki.PagesStats(pageViewsForDays);
        var pageViewStats = PageViewStats.From(wikiStats.Result); // kj2 .Result
        return new object[]
        {
            string.Format(DescriptionFormat, pageViewsForDays, timeline.UtcNow, pageViewStats.Length),
            "",
            PageViewStats.TabularData(pageViewStats)
        };
    }
}