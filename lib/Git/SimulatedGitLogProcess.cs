using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Git;

public record SimulatedGitLogProcess(DaySpan DaySpan, GitLogCommit[] Commits) : IProcessSimulationSpec
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
    /// </summary>
    private const string RoundTripFormat = "o";

    public bool Matches(string executableFilePath, string workingDirPath, string[] arguments)
        => arguments.Any(
            arg => arg.Contains("git log") && arg.Contains(
                GitLog.GitLogParamsStringCommitRange(DaySpan)));

    public List<string> StdOutLines => Commits
        .Select(GetStdOutLines)
        .Aggregate((acc, commitLines) =>
            acc
                .Concat(MoreEnumerable.Return(GitLog.Delimiter))
                .Concat(commitLines).ToList()
        );

    public override string ToString()
        => $"{nameof(SimulatedGitLogProcess)}: " +
           $"GitLogParamsStringCommitRange: {GitLog.GitLogParamsStringCommitRange(DaySpan)}";

    private static List<string> GetStdOutLines(GitLogCommit commit)
    {

        return new List<string>
            {
                commit.Author,
                commit.Date.ToString(RoundTripFormat)
            }.Concat(
                commit.Stats
                    .Select(
                        stat =>
                            $"{stat.Insertions}\t{stat.Deletions}\t{stat.Path}")
            )
            .ToList();
    }
}