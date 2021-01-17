using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;

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

        public async Task<ImmutableArray<GitLogCommit>> GetAuthorChangesStats2(
            int? sinceDays = null,
            DateTime? since = null,
            DateTime? before = null)
        {
            // kja use --numstat instead of --shortstat
            // Reference:
            // https://git-scm.com/docs/git-log#_commit_limiting
            // https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---statltwidthgtltname-widthgtltcountgt
            // SOQ: How can I calculate the number of lines changed between two commits in GIT?
            // A: https://stackoverflow.com/a/2528129/986533
            string sinceDaysStr = sinceDays != null ? " --since=" + sinceDays + ".days" : string.Empty;
            string beforeStr    = before != null ? " --before=" + before.Value.ToShortDateString() : string.Empty;
            string sinceStr     = since != null ? " --since=" + since.Value.ToShortDateString() : string.Empty;
            var    delimiter    = ";";
            var command =
                $"git log{sinceDaysStr}{sinceStr}{beforeStr} " +
                $"--ignore-all-space --ignore-blank-lines " +
                $"--pretty=\"{delimiter}%n%an%n%as\" " +
                $"--numstat";

            return
                (await _repo.GetStdOutLines(command))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Split(delimiter)
                .Select(commit => new GitLogCommit(commit.ToArray()))
                .ToImmutableArray();
        }

        public async Task<List<GitAuthorChangeStats>> GetAuthorChangesStats(
            int? sinceDays = null,
            DateTime? since = null,
            DateTime? before = null)
        {
            // kja use --numstat instead of --shortstat
            // Reference:
            // https://git-scm.com/docs/git-log#_commit_limiting
            // https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---statltwidthgtltname-widthgtltcountgt
            // SOQ: How can I calculate the number of lines changed between two commits in GIT?
            // A: https://stackoverflow.com/a/2528129/986533
            string sinceDaysStr = sinceDays != null ? " --since=" + sinceDays + ".days" : string.Empty;
            string beforeStr    = before != null ? " --before=" + before.Value.ToShortDateString() : string.Empty;
            string sinceStr     = since != null ? " --since=" + since.Value.ToShortDateString() : string.Empty;
            var command =
                $"git log{sinceDaysStr}{sinceStr}{beforeStr} --pretty=\"%an\" --shortstat --ignore-all-space --ignore-blank-lines";
            List<string> stdOutLines = await _repo.GetStdOutLines(command);

            stdOutLines.RemoveAll(string.IsNullOrWhiteSpace);
            RemoveLogEntriesWithNoLineChanges(stdOutLines);

            // Assert that for each commit log there are two lines (one with author, one with stats).
            Debug.Assert(stdOutLines.Count % 2 == 0);

            List<GitAuthorChangeStats> changesStats = Enumerable.Range(0, stdOutLines.Count / 2)
                .Select(index =>
                (
                    author: stdOutLines[index * 2],
                    stats: stdOutLines[index * 2 + 1]
                ).ToGitAuthorChangeStats())
                .ToList();

            return changesStats;
        }

        public async Task<List<GitFileChangeStats>> GetFileChangesStats()
        {
            List<string> stdOutLines =
                await _repo.GetStdOutLines(
                    $"git log --since={_days}.days --format= --numstat --ignore-all-space --ignore-blank-lines");
            return stdOutLines.Select(line => line.ToGitFileChangeStats()).ToList();
        }

        private static void RemoveLogEntriesWithNoLineChanges(List<string> stdOutLines)
        {
            for (var i = 0; i < stdOutLines.Count; i++)
            {
                if (i % 2 == 0) // assert: If true, stdOutLines[i] is an author line and [i+1] is a stats line.
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