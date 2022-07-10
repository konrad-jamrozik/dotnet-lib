using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.AzureDevOps.Tests;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools.Tests;

public record ReportTestsData
{
    private readonly ValidWikiPagesStatsFixture _fix;

    public ReportTestsData()
    {
        _fix = new ValidWikiPagesStatsFixture();
        CommitsLogs = GetCommitsLogs();
        ExpectedRows = new Dictionary<string, object[][]>
        {
            [nameof(GitAuthorsStatsReportTests)] = new[] {
                new object[] { 1, "AuthorC :fire::fire::fire:", 4, 200+300+601+400, 5+82+7+13 },
                new object[] { 2, "AuthorA :fire::fire:", 1, 100, 10 },
                new object[] { 3, "AuthorB :fire:", 1, 77, 7 },
            },
            // kj2-testdata just terrible duplication of test data fixtures
            [nameof(GitFilesStatsReportTests)] = new[] {
                new object[]
                    { 1, WikiPageLink.FromFileSystemPath("Foo/bar601_7.md").ToString(), 601, 7 },
                new object[]
                    { 2, WikiPageLink.FromFileSystemPath("Foo/bar400_13.md").ToString(), 400, 13 },
                new object[]
                {
                    3, WikiPageLink.FromFileSystemPath("Qux/Corge377_89.md").ToString(), 377, 89
                },
                new object[]
                    { 4, WikiPageLink.FromFileSystemPath("Foo/bar200_5.md").ToString(), 200, 5 },
                new object[]
                    { 5, WikiPageLink.FromFileSystemPath("Foo/bar100_10.md").ToString(), 100, 10 }
            },
            [nameof(PageViewStatsReportTests)] = new[] {
                new object[] { 1, new WikiPageLink("/Foo/Baz").ToString(), 182 },
                new object[] { 2, new WikiPageLink("/Foo").ToString(), 70 },
                new object[] { 3, new WikiPageLink("/Qux/Quux/Quuz").ToString(), 28 },
                new object[] { 4, new WikiPageLink("/Home").ToString(), 24 },
                new object[] { 5, new WikiPageLink("/Foo/Bar").ToString(), 16 }
            }
        };
    }

    public readonly GitLogCommit[] CommitsLogs; // kj2 unused

    public WikiPageStats[] WikiPagesStats(int daysOffset = 0) => _fix.WikiPagesStats(daysOffset).ToArray();

    public readonly Dictionary<string, object[][]> ExpectedRows;

    public GitLogCommit[] GetCommitsLogs() => GetCommitsLogs(new SimulatedTimeline().UtcNow);

    public GitLogCommit[] GetCommitsLogs(DateTime date) => new GitLogCommit[] 
    {
        new("AuthorA", date, new GitLogCommit.Numstat[] { new(100, 10, "Foo/bar100_10.md") }),
        new("AuthorB", date, new GitLogCommit.Numstat[] 
        {
            new(77, 7, "Qux/Corge377_89.md")
        }),
        new("AuthorC", date, new GitLogCommit.Numstat[]
        {
            new(200, 5, "Foo/bar200_5.md"),
            new(300, 82, "Qux/Corge377_89.md"),
            new(601, 7, "Foo/bar601_7.md"),
            new(400, 13, "Foo/bar400_13.md")
        })
    };
}