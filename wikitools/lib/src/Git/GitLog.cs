using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Wikitools.Lib.Git
{
    public class GitLog
    {
        private readonly GitRepository _repo;
        private readonly int _days;

        public GitLog(GitRepository repo, int days)
        {
            _repo = repo;
            _days = days;
        }

        public async Task<List<GitChangeStats>> GetChangesStats()
        {
            // Reference:
            // https://git-scm.com/docs/git-log#_commit_limiting
            // https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---statltwidthgtltname-widthgtltcountgt
            // SOQ: How can I calculate the number of lines changed between two commits in GIT?
            // A: https://stackoverflow.com/a/2528129/986533
            List<string> stdOutLines =
                await _repo.GetStdOutLines($"git log --since={_days}.days --pretty=\"%an\" --shortstat");

            stdOutLines.RemoveAll(string.IsNullOrWhiteSpace);
            RemoveLogEntriesWithNoLineChanges(stdOutLines);

            // Assert that for each commit log there are two lines: one with author, one with stats.
            Debug.Assert(stdOutLines.Count % 2 == 0);

            List<GitChangeStats> changesStats = Enumerable.Range(0, stdOutLines.Count / 2)
                .Select(index => 
                    (
                        author: stdOutLines[index * 2],
                        stats: stdOutLines[index * 2 + 1]
                    ).ToGitChangeStats())
                .ToList();

            return changesStats;
        }

        private static void RemoveLogEntriesWithNoLineChanges(List<string> stdOutLines)
        {
            for (var i = 0; i < stdOutLines.Count; i++)
            {
                if (i % 2 == 0) // Author line
                    // Known limitation: ArgumentOutOfRangeException on empty log.
                    if (!(stdOutLines[i + 1].Contains("(+)") || stdOutLines[i + 1].Contains("(-)")))
                    {
                        // Remove the author line of entry with no changes.
                        stdOutLines.RemoveAt(i);
                        // Rewind the loop counter as the indexes have shifted due to removal.
                        i--;
                    }
            }
        }
    }
}