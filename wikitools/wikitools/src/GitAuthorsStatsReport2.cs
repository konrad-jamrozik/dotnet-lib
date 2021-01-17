using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record GitAuthorsStatsReport2
        (ITimeline Timeline, int Days, GitLogCommit[] Commits) : MarkdownDocument
    {
        public override List<object> Content =>
            new()
            {
                $"Git contributions since last {Days} days as of {Timeline.UtcNow}",
                "",
                new TabularData2(Rows(Commits))
            };

        private static (string[] headerRow, object[][] rows) Rows(GitLogCommit[] commits)
        {
            var statsByAuthor = SumByAuthor(commits)
                .OrderByDescending(s => s.insertions + s.deletions)
                .Where(s => !s.author.Contains("Konrad J"))
                .ToArray()[..5];

            var rows = statsByAuthor
                .Select((s, i) => new object[]
                    { i + 1, s.author, s.filesChanged, s.insertions, s.deletions })
                .ToArray();

            return (headerRow: new[] { "Place", "Author", "Files changed", "Insertions", "Deletions" }, rows);
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