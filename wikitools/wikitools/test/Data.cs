using System.Collections.Generic;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;

namespace Wikitools.Tests
{
    public static class Data
    {
        public static readonly List<GitAuthorChangeStats> AuthorChangesStats = new()
        {
            new("AuthorA", 1, 100, 10),
            new("AuthorC", 4, 2000, 22),
            new("AuthorA", 1, 200, 20),
            new("AuthorA", 1, 300, 30),
            new("AuthorB", 11, 10000, 101),
            new("AuthorB", 20, 20000, 202),
            new("AuthorB", 36, 30000, 303),
            new("AuthorC", 2, 1000, 11),
            new("AuthorC", 6, 3000, 33)
        };

        private static readonly List<List<object>> AuthorsStatsReportRows = new()
        {
            new() { 1, "AuthorB", 60000, 606 },
            new() { 2, "AuthorC", 6000, 66 },
            new() { 3, "AuthorA", 600, 60 }
        };

        public static readonly List<GitFileChangeStats> FileChangesStats = new()
        {
            new("insertsOnly.txt", 50, 0),
            new("deletionsOnly.txt", 0, 40),
            new("moreInserts.txt", 70, 48),
            new("moreDeletions.txt", 13, 16),
            new("maxStatsSum.txt", 68, 68),
            new("minStatsSum.txt", 6, 6),
            new("maxInserts.txt", 120, 3),
            new("maxDeletions.txt", 2, 119)
        };

        private static readonly List<List<object>> FilesStatsReportRows = new()
        {
            // kja this will need to be expanded once the test can reach the Verify logic.
            new() { 1, "maxStatsSum.txt", 68, 68 }
        };

        public static readonly List<WikiPageStats> PageStats = new()
        {
            new WikiPageStats("/Home", new List<int> { 1, 20, 0, 0 }),
            new WikiPageStats("/Foo", new List<int> { 60, 0, 8, 0 }),
            new WikiPageStats("/Foo/Bar", new List<int> { 6, 8, 0, 0 }),
            new WikiPageStats("/Foo/Baz", new List<int> { 0, 0, 80, 100 }),
            new WikiPageStats("/Qux/Quux/Quuz", new List<int> { 7, 7, 7, 7 })
        };

        private static readonly List<List<object>> PageViewsStatsReportRows = new()
        {
            new() { 1, "/Foo/Baz", 180 },
            new() { 2, "/Foo", 68 },
            new() { 3, "/Qux/Quux/Quuz", 28 },
            new() { 4, "/Home", 21 },
            new() { 5, "/Foo/Bar", 14 }
        };

        private static readonly List<List<object>> EmptyRows = new();

        public static readonly Dictionary<object, object> Expectation = new()
        {
            [AuthorChangesStats] = AuthorsStatsReportRows,
            [FileChangesStats] = FilesStatsReportRows,
            [PageStats] = PageViewsStatsReportRows,
        };
    }
}