using System;
using System.Linq;
using Wikitools.Lib.Data;
using Wikitools.Lib.Git;

namespace Wikitools;

public record GitFileStats(
    int Place,
    string FilePath,
    int Insertions,
    int Deletions)
{
    public static readonly object[] HeaderRow = { "Place", "FilePath", "Insertions", "Deletions" };

    public static GitFileStats[] From(
        GitLogCommit[] commits,
        Func<string, bool>? filePathFilter = null,
        int? top = null)
    {
        filePathFilter ??= _ => true;
        var statsSumByFilePath = SumByFilePath(commits)
            .OrderByDescending(stats => stats.insertions + stats.deletions)
            .Where(stat => filePathFilter(stat.filePath))
            .ToArray();

        statsSumByFilePath = top is not null 
            ? statsSumByFilePath.Take((int) top).ToArray() 
            : statsSumByFilePath;

        var rows = statsSumByFilePath
            .Select(
                (stats, i) => new GitFileStats(
                    i + 1,
                    stats.filePath,
                    stats.insertions,
                    stats.deletions))
            .ToArray();

        return rows;
    }

    private static (string filePath, int insertions, int deletions)[] SumByFilePath(
        GitLogCommit[] commits)
    {
        var fileStats = commits.SelectMany(
            c => c.Stats.Select(s => (s.FilePath, s.Insertions, s.Deletions)));
        var statsByFilePath = fileStats.GroupBy(s => s.FilePath);
        // kj2 return here FileStats with 0ed place that will be changed by the caller.
        var statsSumByFilePath = statsByFilePath.Select(
            pathStats =>
            (
                filePath: pathStats.Key,
                insertions: pathStats.Sum(s => s.Insertions),
                deletions: pathStats.Sum(s => s.Deletions)
            )
        );
        return statsSumByFilePath.ToArray();
    }

    public static TabularData TabularData(GitFileStats[] rows)
    {
        // kj2 same as Wikitools.GitAuthorStats.TabularData
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    private static object[] AsObjectArray(GitFileStats row)
        => new object[]
        {
            row.Place, row.FilePath, row.Insertions, row.Deletions
        };
}