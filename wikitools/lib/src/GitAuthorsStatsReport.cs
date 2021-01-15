using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools.Lib
{
    public record GitAuthorsStatsReport(ITimeline Timeline, AsyncLazy<List<List<object>>> Rows, int Days) : ITabularData
    {
        public const string DescriptionFormat = "Git contributions since last {0} days as of {1}";
        public static readonly List<object> HeaderRowLabels = new() { "Place", "Author", "Insertions", "Deletions" };

        public GitAuthorsStatsReport(ITimeline timeline, GitLog gitLog, int days)
            : this(
            timeline,
            GetRows(gitLog).AsyncLazy(),
            days) { }

        private static async Task<List<List<object>>> GetRows(GitLog gitLog)
        {
            var changesStats = await gitLog.GetAuthorChangesStats();

            List<GitAuthorChangeStats> authorsStatsOrdered = changesStats.SumByAuthor()
                .OrderByDescending(authorStats => authorStats.Insertions + authorStats.Deletions)
                .ToList();

            List<List<object>> rows = Enumerable.Range(0, authorsStatsOrdered.Count)
                .Select(i => new List<object> { i + 1, authorsStatsOrdered[i].Author, authorsStatsOrdered[i].Insertions, authorsStatsOrdered[i].Deletions })
                .ToList();


            return rows;
        }

        public Task<string> GetDescription() => Task.FromResult(string.Format(DescriptionFormat, Days, Timeline.UtcNow));
        public List<object> HeaderRow => HeaderRowLabels;
        public async Task<List<List<object>>> GetRows() => await Rows.Value;
    }
}