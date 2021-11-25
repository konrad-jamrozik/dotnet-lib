using System;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Data;

namespace Wikitools
{
    public record GitAuthorsStatsReport : MarkdownDocument
    {
        public const string DescriptionFormat = "Git contributions since last {0} days as of {1}";

        public GitAuthorsStatsReport(
            ITimeline timeline,
            Task<GitLogCommit[]> commits,
            int days,
            int? top = null,
            Func<string, bool>? authorFilter = null) : base(
            GetContent(timeline, days, commits, authorFilter ?? (_ => true), top)) { }

        public GitAuthorsStatsReport(
            ITimeline timeline,
            int days,
            GitAuthorStats[] stats) : base(
            GetContent(timeline, days, stats)) { }

        private static object[] GetContent(ITimeline timeline, int days, GitAuthorStats[] stats) =>
            new object[]
            {
                string.Format(DescriptionFormat, days, timeline.UtcNow),
                "",
                new TabularData(Rows(stats))
            };

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
            GitAuthorStats[] rows = GitAuthorStats.AuthorsStatsFrom(commits, authorFilter, top);

            // kj2 Rows conversion to object[]: instead of this conversion, TabularData should
            // handle not only object[][], but also arbitrary_record[], and use reflection
            // to convert this record into a an array of objects[].
            var rowsAsObjectArrays = rows.Select(GitAuthorStats.AsObjectArray).ToArray();

            return (headerRow: GitAuthorStats.HeaderRow, rowsAsObjectArrays);
        }

        private static (object[] headerRow, object[][] rows) Rows(
            GitAuthorStats[] rows)
        {
            // kj2 Rows conversion to object[]: instead of this conversion, TabularData should
            // handle not only object[][], but also arbitrary_record[], and use reflection
            // to convert this record into a an array of objects[].
            var rowsAsObjectArrays = rows.Select(GitAuthorStats.AsObjectArray).ToArray();

            return (headerRow: GitAuthorStats.HeaderRow, rowsAsObjectArrays);
        }
    }
}