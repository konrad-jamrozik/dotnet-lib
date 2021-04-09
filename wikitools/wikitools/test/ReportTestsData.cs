using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Tests;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools.Tests
{
    public record ReportTestsData
    {
        public ReportTestsData()
        {
            var fix = new ValidWikiPagesStatsFixture();
            CommitsLogs = GetCommitsLogs();
            PagesStats = fix.PagesStats().ToArray();
            ExpectedRows = new()
            {
                [(nameof(GitAuthorsStatsReportTests), CommitsLogs)] = new[] {
                    new object[] { 1, "AuthorC", 4, 200+300+601+400, 5+82+7+13 },
                    new object[] { 2, "AuthorA", 1, 100, 10 },
                    new object[] { 3, "AuthorB", 1, 77, 7 },
                },
                [(nameof(GitFilesStatsReportTests), CommitsLogs)] = new[] {
                    new object[] { 1, "/Foo/bar601_7.md", 601, 7 },
                    new object[] { 2, "/Qux/Corge377_89.md", 377, 89 },
                    new object[] { 3, "/Foo/bar400_13.md", 400, 13 },
                    new object[] { 4, "/Foo/bar200_5.md", 200, 5 },
                    new object[] { 5, "/Foo/bar100_10.md", 100, 10 },
                },
                [(nameof(PagesViewsStatsReportTests), PagesStats)] = new[] {
                    new object[] { 1, "/Foo/Baz", 180 },
                    new object[] { 2, "/Foo", 68 },
                    new object[] { 3, "/Qux/Quux/Quuz", 28 },
                    new object[] { 4, "/Home", 21 },
                    new object[] { 5, "/Foo/Bar", 14 }
                }
            };
        }

        public readonly GitLogCommit[] CommitsLogs;

        public readonly WikiPageStats[] PagesStats;

        public readonly Dictionary<(string className, object input), object[][]> ExpectedRows;

        private static GitLogCommit[] GetCommitsLogs() => GetCommitsLogs(new SimulatedTimeline().UtcNow);

        private static GitLogCommit[] GetCommitsLogs(DateTime date) => new GitLogCommit[] 
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
    }
}