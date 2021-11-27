using System;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Data;

namespace Wikitools;

public record PageViewStats(
    int Place,
    string FilePath,
    int Views)
{
    public static readonly object[] HeaderRow = { "Place", "Path", "Views" };

    public static PageViewStats[] From(
        ValidWikiPagesStats stats,
        int? top = null)
    {
        var pathsStats = stats 
            .Select(pageStats =>
                (
                    path: pageStats.Path,
                    views: pageStats.DayStats.Sum(s => s.Count)
                )
            )
            .Where(stat => stat.views > 0)
            .OrderByDescending(stat => stat.views)
            // kj2 make this into extension method and replace everywhere, also for "top is not null"
            .Take(top != null ? new Range(0, (int)top) : Range.All)
            .ToArray();

        var rows = pathsStats
            .Select((path, i) => new PageViewStats(i + 1, path.path, path.views))
            .ToArray();

        return rows;
    }

    public static TabularData TabularData(PageViewStats[] rows)
    {
        // kj2 same as Wikitools.GitAuthorStats.TabularData
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    private static object[] AsObjectArray(PageViewStats row)
        => new object[]
        { row.Place, row.FilePath, row.Views };
}