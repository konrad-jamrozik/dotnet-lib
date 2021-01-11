using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Git
{
    public class GitLogSimulation : IProcessSimulationSpec
    {
        private readonly List<GitChangeStats> _changesStats;
        private readonly int _sinceDays;

        public GitLogSimulation(List<GitChangeStats> changesStats, int sinceDays)
        {
            _changesStats = changesStats;
            _sinceDays = sinceDays;
        }

        public bool Matches(string executableFilePath, string workingDirPath, string[] arguments) 
            => arguments.Any(arg => arg.Contains($"git log --since={_sinceDays}.days --pretty=\"%an\" --shortstat"));

        public List<string> StdOutLines => _changesStats.FromGitLogStdOutLines();
    }
}