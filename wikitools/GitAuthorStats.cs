using System;
using System.Linq;
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

    public static GitAuthorStats[] AuthorsStatsFrom(
        GitLogCommit[] commits,
        Func<string, bool> authorFilter,
        int? top)
    {
        var statsSumByAuthor = SumByAuthor(commits)
            .OrderByDescending(s => s.insertions + s.deletions)
            .Where(s => authorFilter(s.author))
            .ToArray();

        statsSumByAuthor = top is not null
            ? statsSumByAuthor.Take((int)top).ToArray()
            : statsSumByAuthor;

        var rows = statsSumByAuthor
            .Select(
                (data, i) => new GitAuthorStats(
                    i + 1,
                    data.author,
                    data.filesChanged,
                    data.insertions,
                    data.deletions))
            .ToArray();
        return rows;
    }

    private static (string author, int filesChanged, int insertions, int deletions)[]
        SumByAuthor(GitLogCommit[] commits)
    {
        var commitsByAuthor = commits.GroupBy(commit => commit.Author);
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

    public static object[] AsObjectArray(GitAuthorStats row)
    {
        return new object[]
        {
            row.Place, row.AuthorName, row.FilesChanges, row.Insertions, row.Deletions
        };
    }
}