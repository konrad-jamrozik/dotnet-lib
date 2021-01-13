using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools.Lib
{
    public record GitFilesStatsReport : ITabularData
    {
        public const string DescriptionFormat = "Git file changes since last {0} days as of {1}";
        public static readonly List<object> HeaderRowLabels = new() { "Place", "FilePath", "Insertions", "Deletions" };

        private readonly ITimeline _timeline;
        private readonly int _days;
        private readonly AsyncLazy<List<List<object>>> _rows;

        public GitFilesStatsReport(ITimeline timeline, GitLog gitLog, int days)
        {
            _timeline = timeline;
            _days = days;
            _rows = new AsyncLazy<List<List<object>>>(Rows);

            async Task<List<List<object>>> Rows()
            {
                var changesStats = await gitLog.GetFileChangesStats();

                List<GitFileChangeStats> filesStatsOrdered = 
                    changesStats.SumByFilePath()
                        .OrderByDescending(fileStats => fileStats.Insertions + fileStats.Deletions)
                        .ToList();

                List<List<object>> rows = Enumerable.Range(0, filesStatsOrdered.Count)
                    .Select(i => new List<object>
                    {
                        i + 1,
                        filesStatsOrdered[i].FilePath,
                        filesStatsOrdered[i].Insertions,
                        filesStatsOrdered[i].Deletions
                    }).ToList();

                return rows;
            }
        }
        
        public Task<string> GetDescription() => Task.FromResult(string.Format(DescriptionFormat, _days, _timeline.UtcNow));
        public List<object> HeaderRow => HeaderRowLabels;
        public async Task<List<List<object>>> GetRows() => await _rows.Value;
    }
}