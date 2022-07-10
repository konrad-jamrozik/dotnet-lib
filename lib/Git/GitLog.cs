using System;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Git;

public record GitLog(ITimeline Timeline, GitRepository Repo)
{
    // Only this delimiter works. Note, it is prepended with % in the command,
    // so it is --pretty="%% [1]
    // I tried other delimiters, like --pretty="; or --pretty="|
    // They work from terminal but here they return no results. I don't know why.
    // [1] https://git-scm.com/docs/pretty-formats#Documentation/pretty-formats.txt-emem
    public const string Delimiter = "%";

    // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Roundtrip
    private const string GitLogCommitRangeFormat = "o";

    // https://git-scm.com/docs/git-log#_commit_limiting
    public static string GitLogParamsStringCommitRange(DaySpan daySpan)
        => $"--after={((DateTime)daySpan.StartDay).ToString(GitLogCommitRangeFormat)} " +
           // Here the ".AddDays(1)" is necessary for the EndDay to be interpreted as 
           // "_including_ commits made during the EndDay".
           // That is, the value of "--before" has to be the midnight between EndDay and EndDay+1.
           $"--before={((DateTime)daySpan.EndDay.AddDays(1)).ToString(GitLogCommitRangeFormat)} ";

    /// <summary>
    /// Returns commits for 'days' last days, up until the beginning of current day in UTC.
    /// For example, if today UTC is 7/15/2022, then if days = 3, this will return
    /// commits from days 7/12, 7/13 and 7/14, all inclusive.
    ///
    /// Today is excluded because if it was included then calling this method more than once
    /// within one day would possibly return different results, which we don't want.
    /// For example, if first call was at 8 AM UTC and second call at 5 PM UTC,
    /// then if there was at least 1 commit between 8 AM and 5 PM, this method would
    /// return different results, because the call at 5 PM UTC would include the extra commit.
    /// </summary>
    public Task<GitLogCommits> Commits(int days)
    {
        var yesterday = new DateDay(Timeline.UtcNow).Yesterday;
        return GetCommits(daySpan: days.AsDaySpanUntil(yesterday));
    }

    public Task<GitLogCommits> Commits(DaySpan daySpan) 
        => GetCommits(daySpan);

    private static string GitLogCommand(
        DaySpan daySpan,
        string delimiter)
    {
        Contract.Assert(daySpan.Kind == DateTimeKind.Utc);
            
        // Reference:
        // https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History
        // SOQ: How can I calculate the number of lines changed between two commits in GIT?
        // A: https://stackoverflow.com/a/2528129/986533
        var command =
            // https://git-scm.com/docs/git-log
            "git log " +
            "--ignore-all-space --ignore-blank-lines " +
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---numstat
            "--numstat " +
            GitLogParamsStringCommitRange(daySpan) +
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---prettyltformatgt
            // https://git-scm.com/docs/pretty-formats
            $"--pretty=\"%{delimiter}%n%an%n%ad\" " +
            // https://git-scm.com/docs/git-log#Documentation/git-log.txt---dateltformatgt
            "--date=iso-strict";

        // Note: the resulting commits are ordered by timestamp descending.
        return command;
    }

    private async Task<GitLogCommits> GetCommits(DaySpan daySpan)
    {
        var command = GitLogCommand(daySpan, Delimiter);
        var stdOutLines = await Repo.GetStdOutLines(command);
        var commits = stdOutLines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Split(Delimiter)
            .Where(commitLines => commitLines.Any())
            .Select(commitLines => new GitLogCommit(commitLines.ToArray()))
            .ToArray();
        return new GitLogCommits(commits, daySpan);
    }
}