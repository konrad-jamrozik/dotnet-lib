using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Data;
using Wikitools.Lib.Git;

namespace Wikitools;

public record GitAuthorStats(
    string AuthorName,
    int FilesChanges,
    int Insertions,
    int Deletions)
{
    public static readonly object[] HeaderRow =
        { "Place", "Author", "Files changed", "Insertions", "Deletions" };

    public static RankedTop<GitAuthorStats> From(
        GitLogCommits commits,
        Func<string, bool>? authorFilter = null,
        int? top = null,
        bool addIcons = false)
    {
        authorFilter ??= _ => true;
        GitAuthorStats[] statsSumByAuthor = SumByAuthor(commits)
            .OrderByDescending(s => s.Insertions)
            .Where(s => authorFilter(s.AuthorName))
            .ToArray();

        return new RankedTop<GitAuthorStats>(statsSumByAuthor, top);
    }

    private static string AuthorNameWithIcons(
        string authorName,
        int rank)
    {
        int fireAmount = Math.Max(4 - rank, 0);
        return authorName
               + (fireAmount > 0 ? " " : "") 
               // :fire: taken from
               // https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops#emoji
               + string.Join("", ":fire:".Repeat(fireAmount));
    }

    public static TabularData TabularData(RankedTop<GitAuthorStats> rows)
    {
        // kj2 Rows conversion to object[]: instead of this conversion, TabularData should
        // handle not only object[][], but also arbitrary_record[], and use reflection
        // to convert this record into a an array of objects[].
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    private static GitAuthorStats[] SumByAuthor(IEnumerable<GitLogCommit> commits)
    {
        var commitsByAuthor = commits.GroupBy(commit => commit.Author);
        var statsSumByAuthor = commitsByAuthor.Select(authorCommits => new GitAuthorStats(
            authorCommits.Key, 
            // kja this is not true; there may be overlap in the stats, and thus they need to be deduplicated.
            authorCommits.Sum(c => c.Stats.Length), 
            authorCommits.Sum(c => c.Stats.Sum(s => s.Insertions)),
            authorCommits.Sum(c => c.Stats.Sum(s => s.Deletions))
        ));
        return statsSumByAuthor.ToArray();
    }

    private static object[] AsObjectArray((int rank, GitAuthorStats stats) row)
        => new object[]
        {
            row.rank, 
            AuthorNameWithIcons(row.stats.AuthorName, row.rank), 
            row.stats.FilesChanges, 
            row.stats.Insertions,
            row.stats.Deletions
        };
}