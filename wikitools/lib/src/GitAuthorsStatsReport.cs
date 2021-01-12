using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools.Lib
{
    public record GitAuthorsStatsReport : ITabularData
    {
        public const string DescriptionFormat = "Git contributions since last {0} days as of {1}";
        public static readonly List<object> HeaderRowLabels = new() { "Place", "Author", "Insertions", "Deletions" };

        private readonly ITimeline _timeline;
        private readonly int _days;
        private readonly AsyncLazy<List<List<object>>> _rows;

        public GitAuthorsStatsReport(ITimeline timeline, int days, GitLog gitLog)
        {
            _timeline = timeline;
            _days = days;
            _rows = new AsyncLazy<List<List<object>>>(Rows);

            async Task<List<List<object>>> Rows()
            {
                var changesStats = await gitLog.GetChangesStats();

                List<GitChangeStats> authorsStatsOrdered = 
                    changesStats.SumByAuthor()
                    .OrderByDescending(authorStats => authorStats.Insertions + authorStats.Deletions)
                    .ToList();

                List<List<object>> rows = Enumerable.Range(0, authorsStatsOrdered.Count)
                    .Select(i => new List<object>
                    {
                        i + 1,
                        authorsStatsOrdered[i].Author,
                        authorsStatsOrdered[i].Insertions,
                        authorsStatsOrdered[i].Deletions
                    }).ToList();

                return rows;
            }
        }
        
        public Task<string> GetDescription() => Task.FromResult(string.Format(DescriptionFormat, _days, _timeline.UtcNow));
        public List<object> HeaderRow => HeaderRowLabels;
        public async Task<List<List<object>>> GetRows() => await _rows.Value;
    }
}