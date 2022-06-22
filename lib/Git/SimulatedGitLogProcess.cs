﻿using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Git;

public record SimulatedGitLogProcess(ITimeline Timeline, DaySpan daySpan, GitLogCommit[] Commits) : IProcessSimulationSpec
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
    /// </summary>
    private const string RoundTripFormat = "o";

    public bool Matches(string executableFilePath, string workingDirPath, string[] arguments)
        => arguments.Any(
            arg => arg.Contains("git log") && arg.Contains(
                GitLog.GitLogParamsStringCommitRange(daySpan)));

    public List<string> StdOutLines => Commits
        .Select(GetStdOutLines)
        .Aggregate((acc, commitLines) =>
            acc
                .Concat(MoreEnumerable.Return(GitLog.Delimiter))
                .Concat(commitLines).ToList()
        );

    // kja note: this is in Simulated class!
    private static List<string> GetStdOutLines(GitLogCommit commit)
    {
        var authors = commit.Author.WrapInList().Union(commit.Author.WrapInList());
        var authors2 = commit.Author.WrapInList().Concat(commit.Author.WrapInList());

        return new List<string>
            {
                commit.Author,
                commit.Date.ToString(RoundTripFormat)
            }.Union( // kja-git/bug should be Concat instead?
                commit.Stats
                    .Select(
                        stat =>
                            $"{stat.Insertions}\t{stat.Deletions}\t{stat.Path}")
            )
            .ToList();
    }
}