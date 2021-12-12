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
        // Only this delimiter works. Note, it is prepended with % in the command,
        // so it is --pretty="%%
        // I tried other delimiters, like --pretty="; or --pretty="|
        // They work from terminal but here they return no results. I don't know why.
        public const string Delimiter = "%";

        public static DateDay DaysInThePast(DateDay nowDay, int days)
            => nowDay.AddDays(-days);

        public Task<GitLogCommit[]> Commits(int days)
        {
            // kja this utcNowDay will end one day too early.
            // Say the input is 2 days and current day is Jan 10th.
            // The expectation here is the commits should come from Jan 8th 00:00 to
            // Jan 11th 00:00 (i.e. end of Jan 10th)
            // but now it will end at Jan 10th 00:00.
            var utcNowDay = new DateDay(Timeline.UtcNow);
            return GetCommits(after: DaysInThePast(utcNowDay, days), before: utcNowDay);
        }

        // kja this should really be DaySpan
        public Task<GitLogCommit[]> Commits(DateDay after, DateDay before)
            => GetCommits(after: after, before: before);

        private static string GitLogCommand(
            DateDay afterDate,
            DateDay beforeDate,
            string delimiter)
        {
            Contract.Assert(afterDate.Kind == DateTimeKind.Utc);
            Contract.Assert(beforeDate.Kind == DateTimeKind.Utc);
            
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Roundtrip
            var roundtripFormat = "o";

            // Reference:
            // https://git-scm.com/docs/git-log#_commit_limiting
            // https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---statltwidthgtltname-widthgtltcountgt
            // SOQ: How can I calculate the number of lines changed between two commits in GIT?
            // A: https://stackoverflow.com/a/2528129/986533
            var command =
                "git log " +
                $"--after={((DateTime)afterDate).ToString(roundtripFormat)} " +
                $"--before={((DateTime)beforeDate).ToString(roundtripFormat)} " +
                "--ignore-all-space --ignore-blank-lines " +
                $"--pretty=\"%{delimiter}%n%an%n%as\" " +
                "--numstat --date=iso";
            return command;
        }

        private async Task<GitLogCommit[]> GetCommits(
            DateDay after,
            DateDay before)
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
            return commits;
        }
    }
}