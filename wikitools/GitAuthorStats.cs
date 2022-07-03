using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Data;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using ME = MoreLinq.MoreEnumerable;

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
        int top,
        string[]? excludedAuthors = null,
        string[]? excludedPaths = null)
    {
        GitAuthorStats[] statsSumByAuthor = SumByAuthor(commits, excludedPaths)
            .OrderByDescending(stats => stats.Insertions)
            .WhereNotContains(stats => stats.AuthorName, excludedAuthors)
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
        // kj2-report Rows conversion to object[]: instead of this conversion, TabularData should
        // handle not only object[][], but also arbitrary_record[], and use reflection
        // to convert this record into a an array of objects[].
        var rowsAsObjectArrays = rows.Select(AsObjectArray).ToArray();

        return new TabularData((headerRow: HeaderRow, rowsAsObjectArrays));
    }

    public static GitAuthorStats[] SumByAuthor(
        IEnumerable<GitLogCommit> commits,
        string[]? excludedPaths = null)
    {
        var commitsByAuthor = commits.GroupBy(commit => commit.Author);
        var statsSumByAuthor = commitsByAuthor.Select(authorCommits =>
        {
            var numstats = authorCommits.SelectMany(c => c.Stats).ToList();
            var numstatsLookup = GitLogCommit.Numstat.ByFileNameAfterRenames(numstats);
            var filteredNumstats = numstatsLookup
                .WhereNotContains(stats => stats.Key, excludedPaths)
                .ToList();

            var authorStats = new GitAuthorStats(
                authorCommits.Key,
                filteredNumstats.Count,
                filteredNumstats.Sum(fileStats => fileStats.Sum(s => s.Insertions)),
                filteredNumstats.Sum(fileStats => fileStats.Sum(s => s.Deletions))
            );
            return authorStats;
        });
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