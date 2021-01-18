using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools.Tests
{
    public record Data
    {
        public Data()
        {
            var commitLogs = CommitsLogs(new SimulatedTimeline().UtcNow);
            ExpectedRows = new()
            {
                [("GitAuthorsStatsReportTests", commitLogs)] = AuthorsReportRows,
                [("GitFilesStatsReportTests", commitLogs)] = FilesReportRows,
                [("PagesViewsStatsReportTests", PagesStats)] = PageViewsStatsReportRows
            };
        }

        public readonly Dictionary<(string className, object input), object[][]> ExpectedRows;


        public GitLogCommit[] CommitsLogs(DateTime date) => new GitLogCommit[] 
        {
            // kja restore this data
            new("AuthorA", date, new GitLogCommit.Numstat[] { new(100, 10, "/Foo/bar100_10.md") }),
            new("AuthorB", date, new GitLogCommit.Numstat[] 
            {
                new(77, 7, "/Qux/Corge377_89.md")
            }),
            new("AuthorC", date, new GitLogCommit.Numstat[]
            {
                new(200, 5, "/Foo/bar200_5.md"),
                new(300, 12, "/Qux/Corge377_19.md"),
                new(501, 7, "/Foo/bar501_7.md"),
                new(400, 13, "/Foo/bar400_13.md")
            })
        };

        private readonly object[][] AuthorsReportRows =
        {
            new object[] { 1, "AuthorC", 4, 200+500+501+400, 5+12+7+13 },
            new object[] { 2, "AuthorA", 1, 100, 10 },
            new object[] { 3, "AuthorB", 1, 77, 7 },
            
        };

        private readonly object[][] FilesReportRows =
        {
            new object[] { 1, "/Foo/bar500_12.md", 500, 12 },
            new object[] { 2, "/Foo/bar501_7.md", 501, 7 },
            new object[] { 3, "/Qux/Corge377_89.md", 577, 89 },
            new object[] { 4, "/Foo/bar400_13.md", 400, 13 },
            new object[] { 5, "/Foo/bar200_5.md", 200, 5 },
            new object[] { 6, "/Foo/bar100_10.md", 100, 10 },
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

        public readonly WikiPageStats[] PagesStats =
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