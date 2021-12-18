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

    public static RankedTop<GitFileStats> From(
        GitLog gitLog,
        int commitDays,
        string[]? excludedPaths = null,
        int? top = null)
    {
        var commits = gitLog.Commits(commitDays).Result; // kj2 .result

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
            row.rank, 
            // kj2 hardcoded "wiki/" in the .Replace. This is not the only place it is used.
            // ----------------
            // kja this won't show correct link for renames. I need to extract it
            // from 'commits' in a call to:
            // var commits = gitLog.Commits(commitDays).Result
            // The path is of form path/prefix/{oldName.md => newName.md} 
            // and so I could build a "rename map" and then based on it,
            // save it to row.stats.FilePath immediately. So its new type will be
            // WikiPageLink, not string anymore.
            //   Possibly I could later consider intermediate type of
            //   GitCommitFilePath.
            // See also: my OneNote, "Debug snippets".
            //
            // Complication: one page might have been renamed multiple times, and in-between
            // the renames, more insertions/deletions might have happened
            //
            // Complication: when doing raw "GitFileStatsReport" reporting everything,
            // it will show the rename entries with 0 insertions and 0 deletions.
            // They will always be broken links, and probably shouldn't show up at all.
            WikiPageLink.FromFileSystemPath(row.stats.FilePath.Replace("wiki/","")).ToString(),
            row.stats.Insertions, 
            row.stats.Deletions
        };
}