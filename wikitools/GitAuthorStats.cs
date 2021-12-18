using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Data;
using Wikitools.Lib.Git;
using ME = MoreLinq.MoreEnumerable;

namespace Wikitools;

public record GitAuthorStats(
    string AuthorName,
    int FilesChanges,
    int Insertions,
    int Deletions)
{
    // kj2 remove "Place" from here.
    public static readonly object[] HeaderRow =
        { "Place", "Author", "Files changed", "Insertions", "Deletions" };

    public static RankedTop<GitAuthorStats> From(
        GitLog gitLog,
        int commitDays,
        string[]? excludedAuthors,
        int top)
    {
        var commits = gitLog.Commits(commitDays).Result; // kj2 .result

        Func<string, bool>? authorFilter = excludedAuthors != null
            ? author => !excludedAuthors.Any(author.Contains)
            : _ => true;

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
               + string.Join("", ME.Repeat(":fire:", fireAmount));
    }

    public static TabularData TabularData(RankedTop<GitAuthorStats> rows)
    {
        // kj2 Rows conversion to object[]: instead of this conversion, TabularData should
        // handle not only object[][], but also arbitrary_record[], and use reflection
        // to convert this record into a an array of objects[].
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    public static GitAuthorStats[] SumByAuthor(IEnumerable<GitLogCommit> commits)
    {
        var commitsByAuthor = commits.GroupBy(commit => commit.Author);
        var statsSumByAuthor = commitsByAuthor.Select(authorCommits => new GitAuthorStats(
            authorCommits.Key, 
            authorCommits.SelectMany(c => c.Stats).DistinctBy(s => s.FilePath).Count(),
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