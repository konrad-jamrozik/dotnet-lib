﻿using System;
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
           // Here the ".AddDays(1)" is necessary for the BeforeDay to be interpreted as 
           // "_including_ commits made during the BeforeDay" vs "_excluding_".
           $"--before={((DateTime)daySpan.EndDay.AddDays(1)).ToString(GitLogCommitRangeFormat)} ";

    // kj2-git need to memoize stuff from it, now that reports call it themselves.
    public Task<GitLogCommits> Commits(int days)
    {
        var utcNowDay = new DateDay(Timeline.UtcNow);
        DateDay after = DaysInThePast(utcNowDay, days);
        // kj2-git/bug currently this will give different results as current day passes,
        // because the end of the range is utcNowDay i.e. today. So calling this at
        // 8 AM UTC will have less commits than 5 PM UTC.
        // What's worse, the day span is "false" in the sense it doesn't take
        // into the account the last day is never a full day.
        return GetCommits(daySpan: new DaySpan(after, utcNowDay));
    }

    public Task<GitLogCommits> Commits(DaySpan daySpan) 
        => GetCommits(daySpan);

    private static DateDay DaysInThePast(DateDay nowDay, int days)
        => nowDay.AddDays(-days);

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