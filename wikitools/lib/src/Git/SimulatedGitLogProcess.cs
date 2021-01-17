using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

namespace Wikitools.Lib.Git
{
    public class SimulatedGitLogProcess : IProcessSimulationSpec
    {
        private readonly int _sinceDays;
        private readonly GitLogCommit[] _commits;

        public SimulatedGitLogProcess(
            int sinceDays,
            // kja abstract this away into an interface that provides StdOutLines
            GitLogCommit[] commits)
        {
            _sinceDays = sinceDays;
            _commits = commits;
        }
        
        public bool Matches(string executableFilePath, string workingDirPath, string[] arguments)
            => arguments.Any(arg => arg.Contains("git log") && arg.Contains($"--since={_sinceDays}.days"));

        public List<string> StdOutLines => _commits
            .Select(GetStdOutLines)
            .Aggregate((acc, commitLines) =>
                acc
                    .Union(MoreLinq.MoreEnumerable.Return(GitLog.Delimiter))
                    .Union(commitLines).ToList()
            );

        private static List<string> GetStdOutLines(GitLogCommit commit)
        {
            return new List<string>
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
}