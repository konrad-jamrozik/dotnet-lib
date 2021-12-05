using System;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Git
{
    public record GitLog(ITimeline Timeline, GitRepository Repo)
    {
        // Only this delimiter works. Note, it is prepended with % in the command, so it is --pretty="%%
        // I tried other delimiters, like --pretty="; or --pretty="|
        // They work from terminal but here they return no results. I don't know why.
        public const string Delimiter = "%";

        public static DateTime AfterDaysToDate(ITimeline timeline, int days)
            => new DateDay(timeline.UtcNow).AddDays(-days);

        public Task<GitLogCommit[]> Commits(int days)
            => GetCommits(after: AfterDaysToDate(Timeline, days));

        public Task<GitLogCommit[]> Commits(DateTime after, DateTime before)
            => GetCommits(after: after, before: before);

        private static string GitLogCommand(
            DateTime? afterDate,
            DateTime? beforeDate,
            string delimiter)
        {
            Contract.Assert(afterDate == null || afterDate.Value.Kind == DateTimeKind.Utc);
            Contract.Assert(beforeDate == null || beforeDate.Value.Kind == DateTimeKind.Utc);
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Roundtrip
            var roundtripFormat = "o";

            string beforeDateStr = beforeDate != null
                ? " --before=" + beforeDate.Value.ToString(roundtripFormat)
                : string.Empty;
            string afterDateStr = afterDate != null
                ? " --after=" + afterDate.Value.ToString(roundtripFormat)
                : string.Empty;

            // Reference:
            // https://git-scm.com/docs/git-log#_commit_limiting
            // https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---statltwidthgtltname-widthgtltcountgt
            // SOQ: How can I calculate the number of lines changed between two commits in GIT?
            // A: https://stackoverflow.com/a/2528129/986533
            var command =
                $"git log{afterDateStr}{beforeDateStr} " +
                "--ignore-all-space --ignore-blank-lines " +
                $"--pretty=\"%{delimiter}%n%an%n%as\" " +
                "--numstat --date=iso";
            return command;
        }

        private async Task<GitLogCommit[]> GetCommits(
            DateTime? after = null,
            DateTime? before = null)
        {
            var command = GitLogCommand(after, before, Delimiter);
            var stdOutLines = await Repo.GetStdOutLines(command);
            var commits = stdOutLines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Split(Delimiter)
                .Where(commitLines => commitLines.Any())
                .Select(commitLines => new GitLogCommit(commitLines.ToArray()))
                .ToArray();
            // kja this should return GitLogCommits, so then there can be made
            // invariant checks on filtering the commits to dates.
            // But this is nontrivial as after/before are nullable,
            // so full day span might not be available, which is currently
            // required by constructor of GitLogCommit.
            return commits;
        }
    }
}