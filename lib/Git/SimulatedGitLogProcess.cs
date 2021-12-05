using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Git
{
    public record SimulatedGitLogProcess(ITimeline Timeline, int AfterDays, GitLogCommit[] Commits) : IProcessSimulationSpec
    {
        public bool Matches(string executableFilePath, string workingDirPath, string[] arguments)
            => arguments.Any(
                arg => arg.Contains("git log") && arg.Contains(
                    $"--after={GitLog.AfterDaysToDate(Timeline, AfterDays):o}"));

        public List<string> StdOutLines => Commits
            .Select(GetStdOutLines)
            .Aggregate((acc, commitLines) =>
                acc
                    .Concat(MoreEnumerable.Return(GitLog.Delimiter))
                    .Concat(commitLines).ToList()
            );

        private static List<string> GetStdOutLines(GitLogCommit commit) =>
            new List<string>
                {
                    commit.Author,
                    commit.Date.ToShortDateString()
                }.Union(
                    commit.Stats
                        .Select(stat =>
                            $"{stat.Insertions}\t{stat.Deletions}\t{stat.FilePath}")
                )
                .ToList();
    }
}