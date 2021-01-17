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
            int? sinceDays = null,
            DateTime? since = null,
            DateTime? before = null)
        {
            var command = GitLogCommand(sinceDays, since, before);
            return
                (await Repo.GetStdOutLines(command))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Split(Delimiter)
                .Where(commit => commit.Any())
                .Select(commit => new GitLogCommit(commit.ToArray()))
                .ToArray();
        }

        public Task<GitLogCommit[]> Commits(int days) => GetCommits(days);

        public Task<GitLogCommit[]> Commits(DateTime startYear, DateTime endYear) =>
            GetCommits(
                // .AddDays(-1) necessary as for "git log" the --since date day is exclusive
                since: startYear.AddDays(-1),
                // .AddYears(1).AddDays(-1), to include the last day of endYear.
                // In "git log" --before date day is inclusive.
                before: endYear.AddYears(1).AddDays(-1)
            );

        private static string GitLogCommand(int? sinceDays, DateTime? since, DateTime? before)
        {
            string sinceDaysStr = sinceDays != null ? " --since=" + sinceDays + ".days" : string.Empty;
            string beforeStr    = before != null ? " --before=" + before.Value.ToShortDateString() : string.Empty;
            string sinceStr     = since != null ? " --since=" + since.Value.ToShortDateString() : string.Empty;
            // Reference:
            // https://git-scm.com/docs/git-log#_commit_limiting
            // https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---statltwidthgtltname-widthgtltcountgt
            // SOQ: How can I calculate the number of lines changed between two commits in GIT?
            // A: https://stackoverflow.com/a/2528129/986533
            var command =
                $"git log{sinceDaysStr}{sinceStr}{beforeStr} " +
                $"--ignore-all-space --ignore-blank-lines " +
                $"--pretty=\"%{Delimiter}%n%an%n%as\" " +
                $"--numstat";
            return command;
        }
    }
}