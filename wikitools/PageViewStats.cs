using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Data;

namespace Wikitools;

public record PageViewStats(string FilePath, int Views)
{
    // kj2 get rid of Place
    public static readonly object[] HeaderRow = { "Place", "Path", "Views" };

    public static RankedTop<PageViewStats> From(
        ValidWikiPagesStats pagesStats,
        int? top = null)
    {
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
        // kj2 same as Wikitools.GitAuthorStats.TabularData
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    private static object[] AsObjectArray((int rank, PageViewStats stats) row)
        => new object[]
        { row.rank, row.stats.FilePath, row.stats.Views };
}