using System.Collections.Generic;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;

namespace Wikitools.Tests
{
    public static class Data
    {
        public static readonly List<GitAuthorChangeStats> ChangesStats = new()
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

        private static readonly List<List<object>> GitAuthorsStatsReportRows = new()
        {
            new() { 1, "AuthorB", 60000, 606 },
            new() { 2, "AuthorC", 6000, 66 },
            new() { 3, "AuthorA", 600, 60 }
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
            [ChangesStats] = GitAuthorsStatsReportRows,
            [PageStats] = PageViewsStatsReportRows,
        };
    }
}