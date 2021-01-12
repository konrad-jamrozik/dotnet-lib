using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.Lib.Git
{
    public class GitRepository
    {
        private readonly GitBashShell _gitBashShell;
        private readonly string _workingDirPath;

        public GitRepository(GitBashShell gitBashShell, string workingDirPath)
        {
            _gitBashShell = gitBashShell;
            _workingDirPath = workingDirPath;
        }

        public Task<List<string>> GetStdOutLines(string gitCmdLine) 
            => _gitBashShell.GetStdOutLines(_workingDirPath, new[] {gitCmdLine});
    }
}
