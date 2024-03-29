using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Data;
using Wikitools.Lib.Primitives;

namespace Wikitools;

public record PageViewStats(string FilePath, int Views)
{
    public static readonly object[] HeaderRow = { "Place", "Path", "Views" };

    public static async Task<RankedTop<PageViewStats>> From(
        ITimeline timeline,
        IAdoWiki wiki,
        DaySpan daySpan,
        int? top = null)
    {
        // days passed to the ADO API call. Must be from 'daySpan' start to
        // current time in UTC.
        int daysForApiCall = daySpan.Until(timeline.UtcNow).Count;

        // kj2-migration this will trigger call to ADO API.
        // Here it is OK, as we are in late execution stage, but I need to ensure
        // this is fixed everywhere, always deferred to the execution stage.
        //
        // Below are previous ideas I had, now obsolete:
        //
        // I might need to fix all Tasks to AsyncLazy to make this work, or by using new Task() and then task.Start();
        // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-5.0#separating-task-creation-and-execution
        // Maybe source generators could help here. See [Cache] and [Memoize] use cases here:
        // https://github.com/dotnet/roslyn/issues/16160
        // 11/17/2021: Or maybe doing stuff like LINQ IEnumerable is enough? IEnumerable and related
        // collections are lazy after all.
        var pagesStats = (await wiki.PagesStats(daysForApiCall)).Trim(daySpan);

        var pathsStats = pagesStats.Select(
                pageStats => new PageViewStats(
                    pageStats.Path,
                    pageStats.DayStats.Sum(s => s.Count)))
            .Where(stat => stat.Views > 0)
            .OrderByDescending(stat => stat.Views);

        var rankedStats = new RankedTop<PageViewStats>(pathsStats, top);
        return rankedStats;
    }

    public static TabularData TabularData(RankedTop<PageViewStats> rows)
    {
        // kj2-report same as Wikitools.GitAuthorStats.TabularData
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    private static object[] AsObjectArray((int rank, PageViewStats stats) row)
        => new object[]
        {
            row.rank,
            new WikiPageLink(row.stats.FilePath).ToString(),
            row.stats.Views
        };
}