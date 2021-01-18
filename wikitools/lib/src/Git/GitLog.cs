using System;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;

namespace Wikitools.Lib.Git
{
    public record GitLog(GitRepository Repo)
    {
        // Only this delimiter works. Note, it is prepended with % in the command, so it is --pretty="%%
        // I tried other delimiters, like --pretty="; or --pretty="|
        // They work from terminal but here they return no results. I don't know why.
        public const string Delimiter = "%";

        public async Task<GitLogCommit[]> GetCommits(
            int? afterDays = null,
            DateTime? after = null,
            DateTime? before = null)
        {
            var command = GitLogCommand(afterDays, after, before);
            return
                (await Repo.GetStdOutLines(command))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Split(Delimiter)
                .Where(commit => commit.Any())
                .Select(commit => new GitLogCommit(commit.ToArray()))
                .ToArray();
        }

        public Task<GitLogCommit[]> Commits(int days) => GetCommits(days);

        public Task<GitLogCommit[]> Commits(DateTime after, DateTime before) =>
            GetCommits(after: after, before: before);

        private static string GitLogCommand(int? afterDays, DateTime? afterDate, DateTime? beforeDate)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Roundtrip
            var roundtripFormat = "o";

            string afterDaysStr = afterDays != null ? $" --after={afterDays}.days" : string.Empty;
            string beforeDateStr = beforeDate != null
                ? " --before=" + beforeDate.Value.ToUniversalTime().ToString(roundtripFormat)
                : string.Empty;
            string afterDateStr = afterDate != null
                ? " --after=" + afterDate.Value.ToUniversalTime().ToString(roundtripFormat)
                : string.Empty;
            // Reference:
            // https://git-scm.com/docs/git-log#_commit_limiting
            // https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---statltwidthgtltname-widthgtltcountgt
            // SOQ: How can I calculate the number of lines changed between two commits in GIT?
            // A: https://stackoverflow.com/a/2528129/986533
            var command =
                $"git log{afterDaysStr}{afterDateStr}{beforeDateStr} " +
                $"--ignore-all-space --ignore-blank-lines " +
                $"--pretty=\"%{Delimiter}%n%an%n%as\" " +
                $"--numstat --date=iso";
            return command;
        }
    }
}