using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools.Lib
{
    // kja write tests for this
    public record GitFilesStatsReport(ITimeline Timeline, AsyncLazy<List<List<object>>> Rows, int Days) : ITabularData
    {
        public const string DescriptionFormat = "Git file changes since last {0} days as of {1}";
        public static readonly List<object> HeaderRowLabels = new() { "Place", "FilePath", "Insertions", "Deletions" };

        public GitFilesStatsReport(ITimeline timeline, GitLog gitLog, int days) :
            this(
                timeline,
                new AsyncLazy<List<List<object>>>(() => GetRows(gitLog)),
                days) { }

        private static async Task<List<List<object>>> GetRows(GitLog gitLog)
        {
            var changesStats = await gitLog.GetFileChangesStats();

            List<GitFileChangeStats> filesStatsOrdered = changesStats.SumByFilePath()
                .OrderByDescending(fileStats => fileStats.Insertions + fileStats.Deletions)
                .ToList();

            List<List<object>> rows = Enumerable.Range(0, filesStatsOrdered.Count)
                .Select(i => new List<object>
                {
                    i + 1, filesStatsOrdered[i].FilePath, filesStatsOrdered[i].Insertions,
                    filesStatsOrdered[i].Deletions
                })
                .ToList();

            return rows;
        }

        public async Task<List<List<object>>> GetRows() => await Rows.Value;

        public Task<string> GetDescription() =>
            Task.FromResult(string.Format(DescriptionFormat, Days, Timeline.UtcNow));

        public List<object> HeaderRow => HeaderRowLabels;
        
    }
}