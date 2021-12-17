using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Data;
using Wikitools.Lib.Git;

namespace Wikitools;

public record GitFileStats(
    string FilePath,
    int Insertions,
    int Deletions)
{
    // kj2 remove "Place" from here.
    public static readonly object[] HeaderRow = { "Place", "File Path", "Insertions", "Deletions" };

    public static RankedTop<GitFileStats> From2(
        GitLog gitLog,
        int commitDays,
        string[]? excludedPaths,
        int top)
    {
        var commits = gitLog.Commits(commitDays).Result; // kj2 .result

        Func<string, bool>? filePathFilter = excludedPaths != null
            ? path => !excludedPaths.Any(path.Contains)
            : null;

        return From(commits, filePathFilter, top);
    }

    public static RankedTop<GitFileStats> From(
        GitLogCommits commits,
        Func<string, bool>? filePathFilter = null,
        int? top = null)
    {
        filePathFilter ??= _ => true;
        GitFileStats[] statsSumByFilePath =
            SumByFilePath(commits)
                .OrderByDescending(stats => stats.Insertions)
                .Where(stat => filePathFilter(stat.FilePath))
                .ToArray();

        return new RankedTop<GitFileStats>(statsSumByFilePath, top);
    }

    private static GitFileStats[] SumByFilePath(IEnumerable<GitLogCommit> commits)
    {
        var fileStats = commits.SelectMany(
            c => c.Stats.Select(s => (s.FilePath, s.Insertions, s.Deletions)));
        var statsByFilePath = fileStats.GroupBy(s => s.FilePath);
        var statsSumByFilePath = statsByFilePath.Select(
            pathStats => new GitFileStats(
                pathStats.Key,
                pathStats.Sum(s => s.Insertions),
                pathStats.Sum(s => s.Deletions))
        );
        return statsSumByFilePath.ToArray();
    }

    public static TabularData TabularData(RankedTop<GitFileStats> rows)
    {
        // kj2 same as Wikitools.GitAuthorStats.TabularData
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    private static object[] AsObjectArray((int rank, GitFileStats stats) row)
        => new object[]
        {
            row.rank, row.stats.FilePath, row.stats.Insertions, row.stats.Deletions
        };
}