using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record GitAuthorsStatsReport2
        (ITimeline Timeline, int Days, GitLogCommit[] Commits) : MarkdownDocument
    {
        public static readonly object[] HeaderRow = { "Place", "Author", "Files changed", "Insertions", "Deletions" };
        public const string DescriptionFormat = "Git contributions since last {0} days as of {1}";

        public override List<object> Content =>
            new()
            {
                string.Format(DescriptionFormat, Days, Timeline.UtcNow),
                "",
                new TabularData2(Rows(Commits))
            };

        private static (object[] headerRow, object[][] rows) Rows(GitLogCommit[] commits)
        {
            var statsByAuthor = SumByAuthor(commits)
                .OrderByDescending(s => s.insertions + s.deletions)
                .Where(s => !s.author.Contains("Konrad J"))
                .ToArray()[..5];

            var rows = statsByAuthor
                .Select((s, i) => new object[]
                    { i + 1, s.author, s.filesChanged, s.insertions, s.deletions })
                .ToArray();

            return (headerRow: HeaderRow, rows);
        }

        private static (string author, int filesChanged, int insertions, int deletions)[] SumByAuthor(
            GitLogCommit[] commits)
        {
            var commitsByAuthor = commits.GroupBy(commit => commit.Author);
            var statsSumByAuthor = commitsByAuthor.Select(authorCommits =>
                (
                    author: authorCommits.Key,
                    filesChanged: authorCommits.Sum(c => c.Stats.Length),
                    insertions: authorCommits.Sum(c => c.Stats.Sum(s => s.Insertions)),
                    deletions: authorCommits.Sum(c => c.Stats.Sum(s => s.Deletions))
                )
            );
            return statsSumByAuthor.ToArray();
        }
    }
}