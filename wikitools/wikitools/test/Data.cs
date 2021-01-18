using System;
using System.Collections.Generic;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools.Tests
{
    public record Data
    {
        public Data()
        {
            CommitsLogs = GetCommitsLogs(new SimulatedTimeline().UtcNow);
            ExpectedRows = new()
            {
                [("GitAuthorsStatsReportTests", CommitsLogs)] = AuthorsReportRows,
                [("GitFilesStatsReportTests", CommitsLogs)] = FilesReportRows,
                [("PagesViewsStatsReportTests", PagesStats)] = PageViewsStatsReportRows
            };
        }

        public GitLogCommit[] CommitsLogs { get; }

        public readonly Dictionary<(string className, object input), object[][]> ExpectedRows;


        public GitLogCommit[] GetCommitsLogs(DateTime date) => new GitLogCommit[] 
        {
            new("AuthorA", date, new GitLogCommit.Numstat[] { new(100, 10, "/Foo/bar100_10.md") }),
            new("AuthorB", date, new GitLogCommit.Numstat[] 
            {
                new(77, 7, "/Qux/Corge377_89.md")
            }),
            new("AuthorC", date, new GitLogCommit.Numstat[]
            {
                new(200, 5, "/Foo/bar200_5.md"),
                new(300, 82, "/Qux/Corge377_89.md"),
                new(601, 7, "/Foo/bar601_7.md"),
                new(400, 13, "/Foo/bar400_13.md")
            })
        };

        public readonly object[][] AuthorsReportRows =
        {
            new object[] { 1, "AuthorC", 4, 200+300+601+400, 5+82+7+13 },
            new object[] { 2, "AuthorA", 1, 100, 10 },
            new object[] { 3, "AuthorB", 1, 77, 7 },
            
        };

        public readonly object[][] FilesReportRows =
        {
            new object[] { 1, "/Foo/bar601_7.md", 601, 7 },
            new object[] { 2, "/Qux/Corge377_89.md", 377, 89 },
            new object[] { 3, "/Foo/bar400_13.md", 400, 13 },
            new object[] { 4, "/Foo/bar200_5.md", 200, 5 },
            new object[] { 5, "/Foo/bar100_10.md", 100, 10 },
        };

        public readonly WikiPageStats[] PagesStats =
        {
            new("/Home", new[] { 1, 20, 0, 0 }),
            new("/Foo", new[] { 60, 0, 8, 0 }),
            new("/Foo/Bar", new[] { 6, 8, 0, 0 }),
            new("/Foo/Baz", new[] { 0, 0, 80, 100 }),
            new("/Qux/Quux/Quuz", new[] { 7, 7, 7, 7 })
        };

        public readonly object[][] PageViewsStatsReportRows = 
        {
            new object[] { 1, "/Foo/Baz", 180 },
            new object[] { 2, "/Foo", 68 },
            new object[] { 3, "/Qux/Quux/Quuz", 28 },
            new object[] { 4, "/Home", 21 },
            new object[] { 5, "/Foo/Bar", 14 }
        };
    }
}