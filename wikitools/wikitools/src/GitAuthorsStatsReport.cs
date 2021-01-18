using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record GitAuthorsStatsReport : MarkdownDocument
    {
        public static readonly object[] HeaderRow = { "Place", "Author", "Files changed", "Insertions", "Deletions" };
        public const string DescriptionFormat = "Git contributions since last {0} days as of {1}";

        public GitAuthorsStatsReport(
            ITimeline timeline,
            Task<GitLogCommit[]> commits,
            int days,
            int? top = null,
            Func<string, bool>? authorFilter = null) : base(
            GetContent(timeline, days, commits, authorFilter ?? (_ => true), top)) { }

        private static async Task<object[]> GetContent(
            ITimeline timeline,
            int days,
            Task<GitLogCommit[]> commits,
            Func<string, bool> authorFilter,
            int? top) =>
            new object[]
            {
                string.Format(DescriptionFormat, days, timeline.UtcNow),
                "",
                new TabularData(Rows(await commits, authorFilter, top))
            };

        private static (object[] headerRow, object[][] rows) Rows(
            GitLogCommit[] commits,
            Func<string, bool> authorFilter,
            int? top)
        {
            var statsSumByAuthor = SumByAuthor(commits)
                .OrderByDescending(s => s.insertions + s.deletions)
                .Where(s => authorFilter(s.author))
                .ToArray();

            statsSumByAuthor = top is not null 
                ? statsSumByAuthor.Take((int) top).ToArray() 
                : statsSumByAuthor;

            var rows = statsSumByAuthor
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