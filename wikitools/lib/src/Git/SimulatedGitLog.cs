using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Git
{
    public class SimulatedGitLog : IProcessSimulationSpec
    {
        private readonly List<GitAuthorChangeStats> _changesStats;
        private readonly int _sinceDays;

        public SimulatedGitLog(List<GitAuthorChangeStats> changesStats, int sinceDays)
        {
            _changesStats = changesStats;
            _sinceDays = sinceDays;
        }

        public bool Matches(string executableFilePath, string workingDirPath, string[] arguments) 
            => arguments.Any(arg => arg.Contains($"git log --since={_sinceDays}.days --pretty=\"%an\" --shortstat"));

        public List<string> StdOutLines => _changesStats.FromGitLogStdOutLines();
    }
}