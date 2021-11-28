using System;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Data;
using Wikitools.Lib.Git;

namespace Wikitools;

public record GitAuthorStats(
    int Place,
    string AuthorName,
    int FilesChanges,
    int Insertions,
    int Deletions)
{
    public static readonly object[] HeaderRow =
        { "Place", "Author", "Files changed", "Insertions", "Deletions" };

    public static GitAuthorStats[] From(
        GitLogCommit[] commits,
        Func<string, bool>? authorFilter = null,
        int? top = null,
        bool addIcons = false)
    {
        authorFilter ??= _ => true;
        var statsSumByAuthor = SumByAuthor(commits)
            .OrderByDescending(s => s.insertions)
            .Where(s => authorFilter(s.author))
            .ToArray();

        statsSumByAuthor = top is not null
            ? statsSumByAuthor.Take((int)top).ToArray()
            : statsSumByAuthor;

        var rows = statsSumByAuthor
            .Select(
                (data, i) => new GitAuthorStats(
                    i + 1,
                    addIcons ? AuthorNameWithIcons(data, i) : data.author,
                    data.filesChanged,
                    data.insertions,
                    data.deletions))
            .ToArray();

        return rows;
    }

    private static string AuthorNameWithIcons(
        (string author, int filesChanged, int insertions, int deletions) data,
        int sortOrder)
    {
        int fireAmount = Math.Max(3 - sortOrder, 0);
        return data.author 
               + (fireAmount > 0 ? " " : "") 
               // :fire: taken from
               // https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops#emoji
               + string.Join("", ":fire:".Repeat(fireAmount));
    }

    public static TabularData TabularData(GitAuthorStats[] rows)
    {
        // kj2 Rows conversion to object[]: instead of this conversion, TabularData should
        // handle not only object[][], but also arbitrary_record[], and use reflection
        // to convert this record into a an array of objects[].
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    private static (string author, int filesChanged, int insertions, int deletions)[]
        SumByAuthor(GitLogCommit[] commits)
    {
        var commitsByAuthor = commits.GroupBy(commit => commit.Author);
        // kj2 return here AuthorStats with 0ed place that will be changed by the caller.
        var statsSumByAuthor = commitsByAuthor.Select(authorCommits =>
            (
                author: authorCommits.Key,
                filesChanged: authorCommits.Sum(c => c.Stats.Length),
                insertions: authorCommits.Sum(c => c.Stats.Sum(s => s.Insertions)),
                deletions: authorCommits.Sum(c => c.Stats.Sum(s => s.Deletions))
            )
        );
        return statsSumByAuthor.ToArray();
    }

    private static object[] AsObjectArray(GitAuthorStats row)
        => new object[]
        {
            row.Place, row.AuthorName, row.FilesChanges, row.Insertions, row.Deletions
        };
}