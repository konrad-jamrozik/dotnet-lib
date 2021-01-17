using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    // kja NEXT
    public record MonthlyStatsReport(ITimeline Timeline, GitLogCommit[] Commits) : MarkdownDocument
    {
        public override List<object> Content =>
            new()
            {
                $"Git file insertions month over month",
                "",
                new TabularData2(GetRows(Commits))
            };

        private static (object[] headerRow, object[][] rows) GetRows(GitLogCommit[] commits)
        {
            var commitsByMonth = commits
                .Where(commit => !commit.Author.Contains("Konrad J"))
                .GroupBy(commit => (commit.Date.Month + (commit.Date.Year-2019)*100));

            // kja temp debug
            // foreach (var month in commitsByMonth)
            // {
            //     foreach (var c in month)
            //     {
            //         Console.Out.WriteLine($"{c.Author} {c.Date.ToShortDateString()} {c.Stats.Sum(s => s.Insertions)}");
            //     }
            // }

            var insertionsByMonth = commitsByMonth.Select(month => (
                month: month.Key,
                insertions: month.Sum(commit => commit.Stats
                            .Where(stat => !stat.FilePath.Contains("/Meta"))
                            .Sum(stat => stat.Insertions))));
            var rows = insertionsByMonth
                .Select(monthData => new object[] { monthData.month, monthData.insertions, "TODO MoM change" })
                .ToArray();

            return (headerRow: new object[] { "Month", "Insertions", "MoM change" }, rows);
        }
    }
}