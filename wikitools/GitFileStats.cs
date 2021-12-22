using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public static async Task<RankedTop<GitFileStats>> From(
        GitLog gitLog,
        int commitDays,
        string[]? excludedPaths = null,
        int? top = null)
    {
        var commits = await gitLog.Commits(commitDays);

        Func<string, bool> filePathFilter = excludedPaths != null
            ? path => !excludedPaths.Any(path.Contains)
            : _ => true;

        GitFileStats[] statsSumByFilePath =
            SumByFilePath(commits)
                .OrderByDescending(stats => stats.Insertions)
                .Where(stat => filePathFilter(stat.FilePath))
                .ToArray();

        return new RankedTop<GitFileStats>(statsSumByFilePath, top);
    }

    public static TabularData TabularData(RankedTop<GitFileStats> rows)
    {
        // kj2 same as Wikitools.GitAuthorStats.TabularData
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    public static GitFileStats[] SumByFilePath(IEnumerable<GitLogCommit> commits)
    {
        var numstats = commits.SelectMany(c => c.Stats).ToList();

        var numstatsLookup = GitLogCommit.Numstat.ByFileNameAfterRenames(numstats);

        var statsSumByFilePath = numstatsLookup.Select(
            pathStats => new GitFileStats(
                pathStats.Key,
                pathStats.Sum(s => s.Insertions),
                pathStats.Sum(s => s.Deletions)));

        return statsSumByFilePath.ToArray();
    }

    private static object[] AsObjectArray((int rank, GitFileStats stats) row)
        => new object[]
        {
            row.rank, 
            // kj2 hardcoded "wiki/" in the .Replace. This is not the only place it is used.
            WikiPageLink.FromFileSystemPath(row.stats.FilePath.Replace("wiki/","")).ToString(),
            row.stats.Insertions, 
            row.stats.Deletions
        };
}