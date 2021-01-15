using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Git
{
    public class SimulatedGitLogProcess : IProcessSimulationSpec
    {
        private readonly List<GitAuthorChangeStats>? _authorsChangesStats;
        private readonly List<GitFileChangeStats>? _filesChangesStats;
        private readonly int _sinceDays;

        public SimulatedGitLogProcess(
            int sinceDays,
            // kja abstract this away into an interface that provides StdOutLines
            List<GitAuthorChangeStats>? authorsChangesStats = null,
            List<GitFileChangeStats>? filesChangesStats = null)
        {
            _sinceDays = sinceDays;
            _authorsChangesStats = authorsChangesStats;
            _filesChangesStats = filesChangesStats;
        }
        
        // kja support the other command too. Don't duplicaty any of these 2 commands: string.Format.
        public bool Matches(string executableFilePath, string workingDirPath, string[] arguments)
            => arguments.Any(arg => arg.Contains($"git log --since={_sinceDays}.days --pretty=\"%an\" --shortstat"));

        // kja forced null coercion
        public List<string> StdOutLines => _authorsChangesStats?.FromGitLogStdOutLines()!;
    }
}