using System;
using System.Collections.Generic;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;

namespace Wikitools.Tests
{
    public class Data
    {
        public Data()
        {
            ExpectedRows = new()
            {
                [("GitAuthorsStatsReportTests", CommitsLogs)] = AuthorsReportRows,
                [("GitFilesStatsReportTests", CommitsLogs)] = FilesReportRows,
                [("PagesViewsStatsReportTests", PageStats)] = PageViewsStatsReportRows
            };
        }

        public readonly Dictionary<(string className, object input), object[][]> ExpectedRows;

        public readonly GitLogCommit[] CommitsLogs =
        {
            // kja restore this data
            new("AuthorA", new DateTime(2020, 1, 1), new GitLogCommit.Numstat[] { new(100, 10, "/Foo/bar.md") })
            // new("AuthorC", 4, 2000, 22),
            // new("AuthorA", 1, 200, 20),
            // new("AuthorA", 1, 300, 30),
            // new("AuthorB", 11, 10000, 101),
            // new("AuthorB", 20, 20000, 202),
            // new("AuthorB", 36, 30000, 303),
            // new("AuthorC", 2, 1000, 11),
            // new("AuthorC", 6, 3000, 33)
        };

        private readonly object[][] AuthorsReportRows =
        {
            new object[] { 1, "AuthorA", 1, 100, 10 }
        };

        private readonly object[][] FilesReportRows =
        {
            new object[] { 1, "/Foo/bar.md", 100, 10 }
        };

        private readonly List<List<object>> AuthorsStatsReportRows = new()
        {
            new() { 1, "AuthorB", 60000, 606 },
            new() { 2, "AuthorC", 6000, 66 },
            new() { 3, "AuthorA", 600, 60 }
        };

        public readonly List<GitFileChangeStats> FileChangesStats = new()
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

        private readonly List<List<object>> FilesStatsReportRows = new()
        {
            // kja this will need to be expanded once the test can reach the Verify logic.
            new() { 1, "maxStatsSum.txt", 68, 68 }
        };

        public readonly WikiPageStats[] PageStats =
        {
            new("/Home", new[] { 1, 20, 0, 0 }),
            new("/Foo", new[] { 60, 0, 8, 0 }),
            new("/Foo/Bar", new[] { 6, 8, 0, 0 }),
            new("/Foo/Baz", new[] { 0, 0, 80, 100 }),
            new("/Qux/Quux/Quuz", new[] { 7, 7, 7, 7 })
        };

        private readonly object[][] PageViewsStatsReportRows = 
        {
            new object[] { 1, "/Foo/Baz", 180 },
            new object[] { 2, "/Foo", 68 },
            new object[] { 3, "/Qux/Quux/Quuz", 28 },
            new object[] { 4, "/Home", 21 },
            new object[] { 5, "/Foo/Bar", 14 }
        };
    }
}