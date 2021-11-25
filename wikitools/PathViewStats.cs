using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Data;

namespace Wikitools;

public record PathViewStats(
    int Place,
    string FilePath,
    int Views)
{
    public const string DescriptionFormat = "Page views since last {0} days as of {1}. Total wiki pages: {2}";
    public static readonly object[] HeaderRow = { "Place", "Path", "Views" };

    public static PathViewStats[] From(
        ValidWikiPagesStats stats)
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
            .ToArray();

        var rows = pathsStats
            .Select((path, i) => new PathViewStats(i + 1, path.path, path.views))
            .ToArray();

        return rows;
    }

    public static TabularData TabularData(PathViewStats[] rows)
    {
        // kj2 same as Wikitools.GitAuthorStats.TabularData
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    private static object[] AsObjectArray(PathViewStats row)
        => new object[]
        { row.Place, row.FilePath, row.Views };
}