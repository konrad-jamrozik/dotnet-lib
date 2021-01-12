using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;

namespace Wikitools.Tests
{
    public static class Data
    {
        public static readonly List<GitChangeStats> ChangesStats = new()
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

        public static readonly List<List<object>> GitAuthorsStatsReportRows = new()
        {
            new() {1, "AuthorB", 60000, 606 },
            new() {2, "AuthorC", 6000, 66 },
            new() {3, "AuthorA", 600, 60 }
        };

        public static List<List<object>> Expectation(List<GitChangeStats> changesStats) =>
            changesStats.SequenceEqual(ChangesStats)
                ? GitAuthorsStatsReportRows
                : throw new InvalidOperationException();
    }
}